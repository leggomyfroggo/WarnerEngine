namespace WarnerEngine.Lib
{
    public struct Index2
    {
        public static readonly Index2 Zero = new Index2(0, 0);

        public int X { get; set; }
        public int Y { get; set; }

        public Index2(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
