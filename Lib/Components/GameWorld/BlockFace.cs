namespace WarnerEngine.Lib.Components.GameWorld
{
    public struct BlockFace
    {
        public static readonly BlockFace Empty = new BlockFace();

        public string TextureKey;
        public int SourceX;
        public int SourceY;

        public BlockFace(string TextureKey, int SourceX, int SourceY)
        {
            this.TextureKey = TextureKey;
            this.SourceX = SourceX;
            this.SourceY = SourceY;
        }

        // TODO: Add support for additional layers(water, decals, etc)
    }
}
