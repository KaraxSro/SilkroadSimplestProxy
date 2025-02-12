using System.Net.Sockets;
using SilkroadSecurityApi;

namespace SilkroadSimplestProxy
{
    internal class Context
    {
        public Socket Socket { get; set; }
        public Security Security { get; set; }
        public TransferBuffer Buffer { get; set; }
        public Security RelaySecurity { get; set; }

        public Context()
        {
            Security = new Security();
            Buffer = new TransferBuffer(8192);
        }
    }
}
