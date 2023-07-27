namespace Littlefoot.Fixture
{
    internal class Fixture
    {
        public Fixture(short startAddress)
        {
            this.StartAddress = startAddress;
        }

        public short StartAddress { get; set; }

        public byte OffsetMaster { get; set; } = 0;
        public byte OffsetStrobe { get; set; } = 1;
        public byte OffsetRed { get; set; } = 2;
        public byte OffsetGreen { get; set; } = 3;
        public byte OffsetBlue { get; set; } = 4;
        public byte OffsetWhite { get; set; } = 5;
    }
}
