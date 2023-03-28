using Microsoft.Xna.Framework;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Graphics
{
    public class DrawCall
    {
        public string TextureID { get; }
        public Rectangle Target { get; }
        public Rectangle Source { get; }
        public Color Tint { get; }

        public DrawCall(string TextureID, Rectangle Target, Rectangle Source, Color? Tint = null)
        {
            this.TextureID = TextureID;
            this.Target = Target;
            this.Source = Source;
            this.Tint = Tint ?? Color.White;
        }

        public void Draw()
        {
            GameService.GetService<IGraphicsService>().DrawSprite(TextureID, Target, Source, Tint);
        }
    }
}
