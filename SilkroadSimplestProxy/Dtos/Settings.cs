namespace SilkroadSimplestProxy.Dtos
{
    internal class Settings
    {
        public string LocalHost { get; set; }
        public int LocalPort { get; set; }
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }

        public void Deconstruct(out string localHost, out int localPort, out string remoteHost, out int remotePort)
        {
            localHost = LocalHost;
            localPort = LocalPort;
            remoteHost = RemoteHost;
            remotePort = RemotePort;
        }
    }
}
