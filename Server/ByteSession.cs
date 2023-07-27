using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace Littlefoot.Server
{
    internal class ByteSession : TcpSession
    {
        public ByteSession(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"TCP session with Id {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"TCP session with Id {Id} disconnected!");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"TCP session caught an error with code {error}");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("Incoming: " + message);

            switch (message)
            {
                case "TAP":
                    TapTempo.Tap();
                    break;
                case "START_STOP":
                    ShowRunner.StartStop();
                    break;
                case "PAUSE_CONTINUE":
                    ShowRunner.PauseContinue();
                    break;
            }

            // If the buffer starts with '!' the disconnect the current session
            //if (message == "!")
            //    Disconnect();
        }
    }
}
