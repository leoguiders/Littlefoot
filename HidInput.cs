using Littlefoot.Server;
using System.Reflection.Metadata;

namespace Littlefoot
{
    internal class HidInput
    {

        internal static void Run(CancellationTokenSource cts)
        {
            var token = cts.Token;
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            while (!token.IsCancellationRequested)
            {
                var key = Console.ReadKey(true);
                // Console.SetCursorPosition(0, 0);

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        cts.Cancel();
                        break;

                    case ConsoleKey.Spacebar:
                        TapTempo.Tap();
                        break;
                }
                Thread.Sleep(1);
            }
        }
    }
}