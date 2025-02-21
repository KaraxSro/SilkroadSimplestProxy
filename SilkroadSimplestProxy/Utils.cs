using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SilkroadSecurityApi;
using SilkroadSimplestProxy.Dtos;

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

        public static Dictionary<int, List<PacketConfig>> GetPacketConfigLookupDict()
        {
            var fileContents = File.ReadAllText("appsettings.json");

            var deserialized = JsonConvert.DeserializeObject<AppSettings>(fileContents);

            var packetConfigs = new List<PacketConfig>();
            packetConfigs.AddRange(deserialized.PacketConfigs.Global);
            packetConfigs.AddRange(deserialized.PacketConfigs.Agent);
            packetConfigs.AddRange(deserialized.PacketConfigs.Gateway);
            packetConfigs.AddRange(deserialized.PacketConfigs.Download);

            return packetConfigs
                .GroupBy(pc => int.Parse(pc.Opcode, System.Globalization.NumberStyles.HexNumber))
                .ToDictionary(g => g.Key, g => g.ToList());
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
