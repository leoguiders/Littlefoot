using Kryz.Tweening;
using Littlefoot.Server;
using System.ComponentModel.DataAnnotations;

namespace Littlefoot
{
    internal class ShowRunner
    {

        static readonly int UpdateFrequency = 1000 / 50 * 10000; // 50 Hz... might be up to 40 Hz for high quality devices
        static long Now = DateTime.UtcNow.Ticks;

        public static long LastBeat { get; private set; }
        public static long NextBeat { get; private set; }

        private static long _tempo = 1000 * 10000;
        public static long Tempo {
            get =>  _tempo;
            set {
                _tempo = value;
                NextBeat = LastBeat + _tempo;
                Console.WriteLine("ShowRunner Tempo set to {0}", value);
            }

        }

        static long LastExec = DateTime.MinValue.Ticks;
        
        
        

        public static void Run(CancellationToken token, ByteServer byteServer)
        {
            var cuelist = SetupCuelist();
            int currentIndex = 0;

            Now = DateTime.UtcNow.Ticks;
            LastBeat = Now;
            NextBeat = LastBeat + Tempo;

            byte[] buffer = new byte[512];

            while (!token.IsCancellationRequested)
            {
                Now = DateTime.UtcNow.Ticks;
                
                if (Now > NextBeat)
                {
                    LastBeat = Now;
                    NextBeat = LastBeat + Tempo;
                    currentIndex = (currentIndex + 1) % cuelist.Count;
                    Console.WriteLine("Step: {0}", currentIndex);
                }

                if (Now - LastExec > UpdateFrequency)
                {
                    LastExec = Now;

                    var currentCue = cuelist[currentIndex];
                    var nextCue = cuelist[(currentIndex + 1) % cuelist.Count];

                    foreach (var cue in currentCue)
                    {
                        var fixture = cue.Fixture;
                        float currentProgress = Math.Max(0, Math.Min(1 , (Now - LastBeat) / (float) Tempo));

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
                            buffer[fixture.StartAddress + fixture.OffsetAmber] = Convert.ToByte(cue.Amber + ((cueN.Amber - cue.Amber) * easedProgress));
                        }

                    }
                    
                    foreach (var session in byteServer.ByteSessions)
                    {
                        session.SendAsync(buffer);
                    }
                }


            }

            Thread.Sleep(1); // gimme some rest
        }

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
                    Amber = 255,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 255,
                    Amber = 255,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 255,
                    Blue = 0,
                    Amber = 255,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 255,
                    Blue = 255,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture4
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 255,
                    Amber = 255,
                    Easing = easing,
                    Fixture = fixture5
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 255,
                    Amber = 255,
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
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture4
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture5
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 255,
                    Green = 0,
                    Blue = 0,
                    Amber = 0,
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
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 255,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 255,
                    Blue = 0,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 255,
                    Amber = 0,
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
                    Amber = 0,
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
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture1
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 90,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture2
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 130,
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture3
                },
                new Cuelist.FixtureSetting
                {
                    Master = 255,
                    Red = 0,
                    Green = 0,
                    Blue = 170,
                    Amber = 0,
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
                    Amber = 0,
                    Easing = easing,
                    Fixture = fixture6
                }
            };
            cuelist.Add(fourth);

            return cuelist;
        }
    }
}
