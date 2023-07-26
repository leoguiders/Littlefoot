using Littlefoot.Server;
using System.Net;

namespace Littlefoot
{
    internal class Main
    {
        readonly CancellationTokenSource cts;
        
        public Main()
        {
            // Create a main token source to be able to abort all running threads
             cts = new CancellationTokenSource();
        }

        internal void Start()
        {
            int port = 8556;
            var byteServer = new ByteServer(IPAddress.Any, port);
            byteServer.Start();

            Thread mainThread = new Thread(
                () => ShowRunner.Run(cts.Token, byteServer));

            Thread inputThread = new Thread(
                () => HidInput.Run(cts));

            mainThread.Start();
            inputThread.Start();
        }

    }
}