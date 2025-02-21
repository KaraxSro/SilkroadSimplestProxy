namespace SilkroadSimplestProxy.Dtos
{
    internal class PacketConfig
    {
        public string Opcode { get; set; }
        public Initiator Initiator { get; set; }
        public string Name { get; set; }
        public bool Hide { get; set; } = false;
    }
}
