using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Structure.Shadows
{
    public struct Shadow
    {
        public Texture2D Texture;
        public Rectangle DestinationRectangle;
        public Rectangle SourceRectangle;
        public float Opacity;
        public float Depth;

        public Shadow(Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity, float Depth)
        {
            this.Texture = Texture;
            this.DestinationRectangle = DestinationRectangle;
            this.SourceRectangle = SourceRectangle;
            this.Opacity = Opacity;
            this.Depth = Depth;
        }

        public void Draw()
        {
            GameService.GetService<IRenderService>()
                .SetDepth(Depth)
                .DrawQuad(Texture, DestinationRectangle, SourceRectangle, Color.White * Opacity);
        }
    }
}
