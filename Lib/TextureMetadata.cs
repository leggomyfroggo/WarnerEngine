namespace ProjectWarnerShared.Lib
{
    public class TextureMetadata
    {
        public readonly string Key;
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public TextureMetadata(string Key, int X, int Y, int Width, int Height)
        {
            this.Key = Key;
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }
    }
}
