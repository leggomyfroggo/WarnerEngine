using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectWarnerShared.Lib.Structure.Shadows
{
    public class ShadowStack
    {
        private const int MAX_SHADOWS = 200;

        private Shadow[] shadows;
        private int totalShadows;

        public ShadowStack()
        {
            shadows = new Shadow[200];
            totalShadows = 0;
        }

        public void ClearStack()
        {
            totalShadows = 0;
        }

        public void PushShadow(Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity, float Depth)
        {
            shadows[totalShadows].Texture = Texture;
            shadows[totalShadows].DestinationRectangle = DestinationRectangle;
            shadows[totalShadows].SourceRectangle = SourceRectangle;
            shadows[totalShadows].Opacity = Opacity;
            shadows[totalShadows].Depth = Depth;
            totalShadows++;
        }

        public void Draw()
        {
            for (int i = 0; i < totalShadows; i++)
            {
                shadows[i].Draw();
            }
        }
    }
}
