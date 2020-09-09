using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Structure.Shadows
{
    public struct AlphaFragment
    {
        public Texture2D Texture;
        public Rectangle DestinationRectangle;
        public Rectangle SourceRectangle;
        public float Opacity;
        public float Depth;
        public float Rotation;
        public Vector2 Origin;
        public Color Tint;
        public bool IsTiling;

        public AlphaFragment(Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity, float Depth, float Rotation, Vector2 Origin, Color Tint, bool IsTiling)
        {
            this.Texture = Texture;
            this.DestinationRectangle = DestinationRectangle;
            this.SourceRectangle = SourceRectangle;
            this.Opacity = Opacity;
            this.Depth = Depth;
            this.Rotation = Rotation;
            this.Origin = Origin;
            this.Tint = Tint;
            this.IsTiling = IsTiling;
        }

        public void Draw(Color? Tint = null)
        {
            Color tint = Tint.HasValue ? Tint.Value : this.Tint;
            GameService.GetService<IRenderService>()
                .SetDepth(Depth)
                .DrawQuad(
                    Texture, 
                    DestinationRectangle, 
                    SourceRectangle, 
                    Tint: tint * Opacity,
                    Origin: Origin,
                    Rotation: Rotation,
                    IsTiling: IsTiling
                );
        }
    }
}
