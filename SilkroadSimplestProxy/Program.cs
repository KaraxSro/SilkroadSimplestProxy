using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SilkroadSecurityApi;
using System.Threading.Tasks;
using log4net;
using SilkroadSimplestProxy.Dtos;
using System.Linq;
using System.Collections.Generic;

namespace SilkroadSimplestProxy
{

    class Program
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(Program));
        private readonly static Dictionary<int, List<PacketConfig>> packetConfigDict = Utils.GetPacketConfigLookupDict();
        static void MainLoop(string localHost, int localPort, string remoteHost, int remotePort, string serverModuleName)
        {
            try
            {
                var localContext = new Context();
                localContext.Security.GenerateSecurity(true, true, true);

                var remoteContext = new Context();

                remoteContext.RelaySecurity = localContext.Security;
                localContext.RelaySecurity = remoteContext.Security;

                Context[] contexts = [localContext, remoteContext];

                using var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(new IPEndPoint(IPAddress.Parse(localHost), localPort));
                server.Listen(1);
                log.Info($"Proxy listening for connections on {localHost}:{localPort}");

                localContext.Socket = server.Accept();

                using (localContext.Socket)
                using (remoteContext.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    remoteContext.Socket.Connect(remoteHost, remotePort);

                    log.Info($"Connected to Silkroad {serverModuleName} on {remoteHost}:{remotePort}");

                    while (true)
                    {
                        // Network input event processing
                        foreach (var context in contexts)
                        {
                            if (!context.Socket.Poll(0, SelectMode.SelectRead))
                            {
                                continue;
                            }

                            try
                            {
                                int count = context.Socket.Receive(context.Buffer.Buffer);

                                if (count == 0)
                                {
                                    throw new Exception("The remote connection has been lost.");
                                }
                                context.Security.Recv(context.Buffer.Buffer, 0, count);
                            }
                            catch (Exception)
                            {
                                // If any error happens restart the proxy
                                return;
                            }
                        }

                        // Logic event processing
                        foreach (var context in contexts)
                        {
                            var packets = context.Security.TransferIncoming();

                            if (packets == null)
                            {
                                continue;
                            }

                            foreach (var packet in packets)
                            {
                                if (packet.Opcode == 0x2001)
                                {
                                    if (context == remoteContext) // ignore local to proxy only
                                    {
                                        context.RelaySecurity.Send(packet); // proxy to remote is handled by API
                                    }
                                }
                                else if (packet.Opcode == 0xA102)
                                {
                                    var result = packet.ReadUInt8();

                                    if (result != 1)
                                    {
                                        continue;
                                    }

                                    var id = packet.ReadUInt32();
                                    var ip = packet.ReadAscii();
                                    var port = packet.ReadUInt16();

                                    var agentProxyPort = localPort + 1;

                                    // When entering Id and Pw, we disconnect from the gateway and connect to the agent, so we create a task for the agent
                                    Task.Run(() =>
                                    {
                                        MainLoop(localHost, agentProxyPort, ip, port, "AgentServer");
                                        log.Info("Disconnected from AgentServer.");
                                        // The proxy started as soon as we connected to AgentServer and disconnected from the GatewayServer, but we show the same notification
                                        log.Info($"Proxy listening for connections on {localHost}:{localPort}");
                                    });
                                    
                                    var newPacket = new Packet(0xA102, true);
                                    newPacket.WriteUInt8(result);
                                    newPacket.WriteUInt32(id);
                                    newPacket.WriteAscii(localHost);
                                    newPacket.WriteUInt16(agentProxyPort);

                                    context.RelaySecurity.Send(newPacket);
                                }
                                else if (packet.Opcode != 0x5000 && packet.Opcode != 0x9000) // We ignore Handshake related packets
                                {
                                    context.RelaySecurity.Send(packet);
                                }
                            }
                        }

                        // Network output event processing
                        foreach (var context in contexts)
                        {
                            if (!context.Socket.Poll(0, SelectMode.SelectWrite))
                            {
                                continue;
                            }

                            var buffers = context.Security.TransferOutgoing();

                            if (buffers == null)
                            {
                                continue;
                            }

                            foreach (var kvp in buffers)
                            {
                                var initiator = context == localContext ? Initiator.Server : Initiator.Client;

                                var buffer = kvp.Key;
                                var packet = kvp.Value;

                                while (buffer.Offset != buffer.Size)
                                {
                                    int count = context.Socket.Send(buffer.Buffer, buffer.Offset, buffer.Size, SocketFlags.None);

                                    buffer.Offset += count;
                                }

                                PrintPacket(initiator, packet);
                            }
                        }

                        // Cycle complete, prevent 100% CPU usage
                        Thread.Sleep(1);
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private static void PrintPacket(Initiator initiator, Packet packet)
        {
            // Check if we have anything for this packet in the config
            packetConfigDict.TryGetValue(packet.Opcode, out List<PacketConfig> packetConfigs);
            var packetConfig = packetConfigs != null ? packetConfigs.SingleOrDefault(pc => pc.Initiator == initiator) : null;

            if (packetConfig != null)
            {
                if(packetConfig.Hide)
                {
                    return;
                }

                log.Warn($"[{packetConfig.Name}]");
            }

            var directionText = initiator == Initiator.Server ? "S->C" : "C->S";
            log.Warn($"[{directionText}]{Utils.GetPacketDataAsString(packet)}");
        }

        static void Main(string[] args)
        {
            var (localHost, localPort, remoteHost, remotePort) = Utils.ParseArgs(args);

            while (true)
            {
                MainLoop(localHost, localPort, remoteHost, remotePort, "GatewayServer");
                Thread.Sleep(1);
            }
        }
    }
}