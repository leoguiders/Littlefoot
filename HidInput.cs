using Littlefoot.Server;
using System.Reflection.Metadata;

namespace Littlefoot
{
    internal class HidInput
    {

        internal enum InputMode
        {
            Normal,
            TapTempo
        }

        static InputMode inputMode = InputMode.Normal;

        internal static void Run(CancellationTokenSource cts)
        {
            var token = cts.Token;
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            List<long>? tapList = new List<long>();
            CancellationTokenSource cancelWatchdog = new CancellationTokenSource();
            Task? tappingWatchDog = null;

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
                        if (inputMode == InputMode.Normal)
                        {
                            inputMode = InputMode.TapTempo;
                            tapList.Clear();
                            long first = DateTime.UtcNow.Ticks;
                            tapList.Add(first);
                            Console.WriteLine("Starting Tap Tempo");
                        } 
                        else if (inputMode == InputMode.TapTempo)
                        {
                            if (tappingWatchDog != null)
                            {
                                cancelWatchdog.Cancel();
                                cancelWatchdog.Dispose();
                            }

                            tapList.Add(DateTime.UtcNow.Ticks);

                            long sum = 0;
                            for (int i = 1; i < tapList.Count; i++)
                            {
                                sum += tapList[i] - tapList[i - 1];
                            }
                            long average = sum / (tapList.Count - 1);
                            long threshold = average * 2 / 10000;

                            Console.WriteLine("Current tempo: {0} bpm (beat length {1}ms)", 60000L / (average / 10000L) , average / 10000);

                            ShowRunner.Tempo = average;

                            tappingWatchDog = Task.Run(async delegate
                            {
                                cancelWatchdog = new CancellationTokenSource();
                                try
                                {
                                      await Task.Delay(TimeSpan.FromMilliseconds(threshold), cancelWatchdog.Token);
                                      inputMode = InputMode.Normal;
                                      Console.WriteLine("InputMode reset to normal");
                                }
                                catch (TaskCanceledException)
                                {
                                    // ignore the TaskCanceledException because it's ok here
                                }
                            });
                        }
                        break;
                }
                Thread.Sleep(1);
            }
        }
    }
}