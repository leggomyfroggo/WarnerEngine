using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Entities;
using WarnerEngine.Lib.Helpers;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Components.GameWorld
{
    public class Block : IWorldSerializable, ISceneEntity, IDraw
    {
        private BackingBox backingBox;

        public int TileSize;

        public int X;
        public int Y;
        public int Z;

        public int Width;
        public int Height;
        public int Depth;

        public BlockFace[] Tiles;

        public Block(int X, int Y, int Z, int Width, int Height, int Depth, BlockFace[] Tiles)
        {
            TileSize = 16;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Width = Width;
            this.Height = Height;
            this.Depth = Depth;
            this.Tiles = Tiles;
            PostSerialize();
        }

        public void OnAdd(Scene ParentScene) { }

        public void OnRemove(Scene ParentScene) { }

        public void Draw()
        {
            IRenderService rs = GameService.GetService<IRenderService>();

            Vector2 projectedPosition = backingBox.GetFullProjectedPosition2();
            Rectangle bounds = new Rectangle(
                (int)projectedPosition.X, 
                (int)projectedPosition.Y, 
                Width * TileSize, 
                (Height + Depth) * TileSize
            );
            Rectangle plotter = new Rectangle(bounds.Left, bounds.Top, TileSize, TileSize);
            foreach (BlockFace face in Tiles)
            {
                if (!face.Equals(BlockFace.Empty))
                {
                    rs.DrawQuad(
                        face.TextureKey,
                        plotter,
                        new Rectangle(face.SourceX, face.SourceY, TileSize, TileSize)
                    );
                }
                plotter.X += TileSize;
                if (plotter.X >= bounds.Right)
                {
                    plotter.X = bounds.Left;
                    plotter.Y += TileSize;
                }
            }
        }

        public BackingBox GetBackingBox()
        {
            return backingBox;
        }

        public bool IsVisible()
        {
            return true;
        }

        public void PostSerialize()
        {
            backingBox = new BackingBox(
                BackingBox.IType.Static,
                X,
                Y,
                Z,
                Width * TileSize,
                Height * TileSize,
                Depth * TileSize
            );
        }
    }
}
