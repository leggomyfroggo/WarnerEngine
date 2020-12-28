namespace WarnerEngine.Lib
{
    public struct Index3
    {
        public static readonly Index3 Zero = new Index3(0, 0, 0);
        public static readonly Index3 XOne = new Index3(1, 0, 0);
        public static readonly Index3 YOne = new Index3(0, 1, 0);
        public static readonly Index3 ZOne = new Index3(0, 0, 1);

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public Index3(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public static Index3 operator +(Index3 Left, Index3 Right)
        {
            return new Index3(Left.X + Right.X, Left.Y + Right.Y, Left.Z + Right.Z);
        }
    }
}
