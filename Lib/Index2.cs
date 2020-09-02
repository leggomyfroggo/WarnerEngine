namespace ProjectWarnerShared.Lib
{
    public struct Index2
    {
        public static readonly Index2 Zero = new Index2(0, 0);

        public int X { get; private set; }
        public int Y { get; private set; }

        public Index2(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
