using System.Net;
using System.Net.Sockets;
using NetCoreServer;

namespace Littlefoot.Server
{
    internal class ByteServer : TcpServer
    {

        public ByteServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override TcpSession CreateSession()
        {
            return new ByteSession(this);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"TCP server caught an error with code {error}");
        }

        public ICollection<TcpSession> ByteSessions => Sessions.Values;
        
    }
}
