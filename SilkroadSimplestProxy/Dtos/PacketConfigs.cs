using System.Collections.Generic;

namespace SilkroadSimplestProxy.Dtos
{
    internal class PacketConfigs
    {
        public IEnumerable<PacketConfig> Global { get; set; } = new List<PacketConfig>();
        public IEnumerable<PacketConfig> Agent { get; set; } = new List<PacketConfig>();
        public IEnumerable<PacketConfig> Gateway { get; set; } = new List<PacketConfig>();
        public IEnumerable<PacketConfig> Download { get; set; } = new List<PacketConfig>();
    }
}
