namespace Littlefoot
{
    internal static class TapTempo
    {

        enum TapMode
        {
            Idle,
            Tapping
        }

        static TapMode _tapMode = TapMode.Idle;

        static List<long> _tapList = new List<long>();
        static CancellationTokenSource _cancelWatchdog = new CancellationTokenSource();
        static Task? _tappingWatchdog = null;


        internal static void Tap()
        {
            if (_tapMode == TapMode.Idle)
            {
                _tapMode = TapMode.Tapping;
                _tapList.Clear();

                long first = DateTime.UtcNow.Ticks;
                _tapList.Add(first);
                Console.WriteLine("Starting Tap Tempo");
            }
            else if (_tapMode == TapMode.Tapping)
            {
                if (_tappingWatchdog != null)
                {
                    _cancelWatchdog.Cancel();
                    _cancelWatchdog.Dispose();
                }

                _tapList.Add(DateTime.UtcNow.Ticks);

                long sum = 0;
                for (int i = 1; i < _tapList.Count; i++)
                {
                    sum += _tapList[i] - _tapList[i - 1];
                }
                long average = sum / (_tapList.Count - 1);
                long threshold = average * 2 / 10000;

                Console.WriteLine("Current tempo: {0} bpm (beat length {1}ms)", 60000L / (average / 10000L), average / 10000);

                ShowRunner.Tempo = average;

                _tappingWatchdog = Task.Run(async delegate
                {
                    _cancelWatchdog = new CancellationTokenSource();
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(threshold), _cancelWatchdog.Token);
                        _tapMode = TapMode.Idle;
                        Console.WriteLine("InputMode reset to normal");
                    }
                    catch (TaskCanceledException)
                    {
                        // ignore the TaskCanceledException because it's ok here
                    }
                });
            }
        }
    }
}
