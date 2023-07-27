using Kryz.Tweening;
using Littlefoot.Cuelist;
using Littlefoot.Server;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Littlefoot
{
    internal class ShowRunner
    {

        enum RunStatus
        {
            Stopped,
            Running,
            Paused
        }

        static readonly int _updateFrequency = 1000 / 50 * 10000; // 50 Hz... might be up to 40 Hz for high quality devices
        static long _now = DateTime.UtcNow.Ticks;

        public static long _lastBeat { get; private set; }
        public static long _nextBeat { get; private set; }

        private static long _tempo = 1000 * 10000;
        public static long Tempo {
            get =>  _tempo;
            set {
                _tempo = value;
                _nextBeat = _lastBeat + _tempo;
                Console.WriteLine("ShowRunner Tempo set to {0}", value);
            }

        }

        private static RunStatus _runStatus = RunStatus.Stopped;
        private static long _restToBeat;
        static long _lastExec = DateTime.MinValue.Ticks;

        static int _currentIndex = 0;
        static Cuelist.Cuelist _cuelist = SetupCuelist();

        public static void Run(CancellationToken token, ByteServer byteServer)
        {

            _now = DateTime.UtcNow.Ticks;
            _lastBeat = _now;
            _nextBeat = _lastBeat + Tempo;

            byte[] buffer = new byte[512];

            while (!token.IsCancellationRequested)
            {

                _now = DateTime.UtcNow.Ticks;

                if (_runStatus == RunStatus.Stopped)
                {
                    if (_now - _lastExec > _updateFrequency)
                    {
                        _lastExec = _now;

                        var currentCue = _cuelist[_currentIndex];
                        foreach (var cue in currentCue)
                        {
                            var fixture = cue.Fixture;
                            float currentProgress = Math.Max(0, Math.Min(1, (_now - _lastBeat) / (float)Tempo));

                            buffer[fixture.StartAddress + fixture.OffsetMaster] = Convert.ToByte(cue.Master + ((0 - cue.Master) * currentProgress));
                            buffer[fixture.StartAddress + fixture.OffsetRed] = Convert.ToByte(cue.Red + ((0 - cue.Red) * currentProgress));
                            buffer[fixture.StartAddress + fixture.OffsetGreen] = Convert.ToByte(cue.Green + ((0 - cue.Green) * currentProgress));
                            buffer[fixture.StartAddress + fixture.OffsetBlue] = Convert.ToByte(cue.Blue + ((0 - cue.Blue) * currentProgress));
                            buffer[fixture.StartAddress + fixture.OffsetWhite] = Convert.ToByte(cue.White + ((0 - cue.White) * currentProgress));
                        }

                        foreach (var session in byteServer.ByteSessions)
                        {
                            session.SendAsync(buffer);
                        }
                    }
                }
                else if (_runStatus == RunStatus.Paused)
                {

                }
                else if (_runStatus == RunStatus.Running)
                {

                    if (_now > _nextBeat)
                    {
                        _lastBeat = _now;
                        _nextBeat = _lastBeat + Tempo;
                        _currentIndex = (_currentIndex + 1) % _cuelist.Count;
                        Console.WriteLine("Step: {0}", _currentIndex);
                    }

                    if (_now - _lastExec > _updateFrequency)
                    {
                        _lastExec = _now;

                        var currentCue = _cuelist[_currentIndex];
                        var nextCue = _cuelist[(_currentIndex + 1) % _cuelist.Count];

                        foreach (var cue in currentCue)
                        {
                            var fixture = cue.Fixture;
                            float currentProgress = Math.Max(0, Math.Min(1, (_now - _lastBeat) / (float)Tempo));

                            var cueN = nextCue.Where(nc => nc.Fixture == cue.Fixture).FirstOrDefault();
                            if (cueN != null)
                            {
                                var easing = cueN.Easing;
                                var cueEasing = EasingFunctions.GetEasingFunction(easing);
                                var easedProgress = cueEasing(currentProgress);
                                buffer[fixture.StartAddress + fixture.OffsetMaster] = Convert.ToByte(cue.Master + ((cueN.Master - cue.Master) * easedProgress));
                                buffer[fixture.StartAddress + fixture.OffsetRed] = Convert.ToByte(cue.Red + ((cueN.Red - cue.Red) * easedProgress));
                                buffer[fixture.StartAddress + fixture.OffsetGreen] = Convert.ToByte(cue.Green + ((cueN.Green - cue.Green) * easedProgress));
                                buffer[fixture.StartAddress + fixture.OffsetBlue] = Convert.ToByte(cue.Blue + ((cueN.Blue - cue.Blue) * easedProgress));
                                buffer[fixture.StartAddress + fixture.OffsetWhite] = Convert.ToByte(cue.White + ((cueN.White - cue.White) * easedProgress));
                            }

                        }

                        foreach (var session in byteServer.ByteSessions)
                        {
                            session.SendAsync(buffer);
                        }
                    }
                }
            }

            Thread.Sleep(1); // gimme some rest
        }

        public static void StartStop()
        {
            if (_runStatus == RunStatus.Stopped)
            {
                _lastBeat = DateTime.UtcNow.Ticks;
                _nextBeat = _lastBeat + Tempo;
                _runStatus = RunStatus.Running;
            }
            else if (_runStatus == RunStatus.Running)
            {
                // _currentIndex = (_currentIndex + 1) % _cuelist.Count;
                // _lastBeat = DateTime.UtcNow.Ticks;
                // _nextBeat = _lastBeat + Tempo;
                _runStatus = RunStatus.Stopped;
            }
        }

        public static void PauseContinue()
        {
            if (_runStatus == RunStatus.Running)
            {
                _runStatus = RunStatus.Paused;
                _restToBeat = _nextBeat - DateTime.UtcNow.Ticks;
                Console.WriteLine("Rest to beat: {0}", _restToBeat);
            } 
            else if (_runStatus == RunStatus.Paused) 
            {
                _nextBeat = DateTime.UtcNow.Ticks + _restToBeat;
                _lastBeat = _nextBeat - Tempo;
                _runStatus = RunStatus.Running;
            }
        }

        #region testdata
        private static Cuelist.Cuelist SetupCuelist()
        {
            Fixture.Fixture fixture1 = new(19);
            Fixture.Fixture fixture2 = new(28);
            Fixture.Fixture fixture3 = new(37);
            Fixture.Fixture fixture4 = new(46);
            Fixture.Fixture fixture5 = new(55);
            Fixture.Fixture fixture6 = new(64);

            var easing = Easing.InOutQuad;

            Cuelist.Cuelist cuelist = new();
            Cuelist.CuelistItem first = new()
            {
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 255,
                    White = 255,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 255,
                    White = 255,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 255,
                    Blue = 0,
                    White = 255,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 255,
                    Blue = 255,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture4
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 255,
                    White = 255,
                    Easing = easing,
                    Fixture = fixture5
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 255,
                    White = 255,
                    Easing = easing,
                    Fixture = fixture6
                }
            };
            cuelist.Add(first);

            Cuelist.CuelistItem second = new()
            {
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture4
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture5
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture6
                }
            };
            cuelist.Add(second);

            Cuelist.CuelistItem third = new()
            {
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 255,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 0,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 255,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture4
                },
                /*new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 0,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture5
                },*/
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 255,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture6
                }
            };
            cuelist.Add(third);


            Cuelist.CuelistItem fourth = new()
            {
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 50,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 90,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 130,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 170,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture4
                },
                /*new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 210,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture5
                },*/
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 250,
                    White = 0,
                    Easing = easing,
                    Fixture = fixture6
                }
            };
            cuelist.Add(fourth);

            return cuelist;
        }
        #endregion
    }
}
