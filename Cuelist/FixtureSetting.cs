using Kryz.Tweening;

namespace Littlefoot.Cuelist
{
    internal class FixtureSetting
    {
        public byte Master { get; internal set; } = 0;
        public byte Red { get; internal set; } = 0;
        public byte Green { get; internal set; } = 0;
        public byte Blue { get; internal set; } = 0;
        public byte Amber { get; internal set; } = 0;
        public Fixture.Fixture Fixture { get; internal set; }
        public Easing Easing { get; internal set; } = Easing.Linear;
    }

}