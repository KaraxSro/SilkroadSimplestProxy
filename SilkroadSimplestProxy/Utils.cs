using System;
using System.IO;
using System.Reflection;
using System.Text;
using SilkroadSecurityApi;

namespace SilkroadSimplestProxy
{
    internal static class Utils
    {
        public static Settings ParseArgs(string[] args)
        {
            try
            {
                return new Settings
                {
                    LocalHost = args[0],
                    LocalPort = int.Parse(args[1]),
                    RemoteHost = args[2],
                    RemotePort = int.Parse(args[3]),
                };
            }
            catch
            {
                Console.WriteLine($"Usage: {Path.GetFileName(Assembly.GetExecutingAssembly().Location)} <{nameof(Settings.LocalHost)}> <{nameof(Settings.LocalPort)}> <{nameof(Settings.RemoteHost)}> <{nameof(Settings.RemotePort)}>");
                throw;
            }
        }

        public static string GetPacketDataAsString(Packet packet)
        {
            var packetBytes = packet.GetBytes();

            var sb = new StringBuilder();
            sb.Append($"[{packet.Opcode:X4}]");
            sb.Append($"[{packetBytes.Length} bytes]");
            
            if(packet.Encrypted)
            {
                sb.Append("[Encrypted]");
            }

            if(packet.Massive)
            {
                sb.Append("[Massive]");
            }

            sb.AppendLine();
            sb.AppendLine(Utility.HexDump(packetBytes));
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
