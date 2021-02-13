namespace WarnerEngine.Lib.UI
{
    public struct UISize
    {
        public static readonly UISize Empty = new UISize();

        public readonly UIEnums.Sizing Sizing;
        public readonly int Size;
        public readonly int Minimum;
        public readonly int MarginStart;
        public readonly int MarginEnd;
        public readonly int PaddingStart;
        public readonly int PaddingEnd;
        public int FullMargin => MarginStart + MarginEnd;
        public int FullPadding => PaddingStart + PaddingEnd;

        public UISize(UIEnums.Sizing Sizing, int Size, int Minimum = 0, int MarginStart = 0, int MarginEnd = 0, int PaddingStart = 0, int PaddingEnd = 0)
        {
            this.Sizing = Sizing;
            this.Size = Size;
            this.Minimum = Minimum;
            this.MarginStart = MarginStart;
            this.MarginEnd = MarginEnd;
            this.PaddingStart = PaddingStart;
            this.PaddingEnd = PaddingEnd;
        }
    }
}
