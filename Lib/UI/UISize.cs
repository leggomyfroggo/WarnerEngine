namespace WarnerEngine.Lib.UI
{
    public struct UISize
    {
        public readonly UIEnums.Sizing Sizing;
        public readonly int Size;
        public readonly int Minimum;
        public readonly int MarginStart;
        public readonly int MarginEnd;
        public int FullMargin => MarginStart + MarginEnd;

        public UISize(UIEnums.Sizing Sizing, int Size, int Minimum = 0, int MarginStart = 0, int MarginEnd = 0)
        {
            this.Sizing = Sizing;
            this.Size = Size;
            this.Minimum = Minimum;
            this.MarginStart = MarginStart;
            this.MarginEnd = MarginEnd;
        }
    }
}
