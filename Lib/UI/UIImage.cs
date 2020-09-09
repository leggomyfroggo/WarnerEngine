using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.UI
{
    public class UIImage : UIElement<UIImage>
    {
        public enum Scaling { Stretch, LetterBox, Window }

        private string textureKey;
        private Scaling scaling;
        private Vector2 windowOffset;

        public UIImage(string Key, UIRenderer UIRendererInstance) : base(Key, UIRendererInstance) { }

        public UIImage SetTextureKey(string TextureKey)
        {
            textureKey = TextureKey;
            return this;
        }

        public UIImage SetScaling(Scaling S)
        {
            scaling = S;
            return this;
        }

        public UIImage SetWindowOffset(Vector2 WindowOffset)
        {
            windowOffset = WindowOffset;
            return this;
        }

        protected override UIImage FinalizeImplementation()
        {
            return this;
        }

        public override void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY)
        {
            Texture2D texture = GameService.GetService<IContentService>().GetxAsset<Texture2D>(textureKey);
            switch (scaling)
            {
                case Scaling.Stretch:
                    GameService.GetService<IRenderService>().DrawQuad(
                        texture,
                        new Rectangle(RenderedX, RenderedY, RenderedWidth, RenderedHeight),
                        new Rectangle(0, 0, texture.Width, texture.Height)
                    );
                    break;
                case Scaling.LetterBox:
                    float elementRatio = (float)RenderedWidth / RenderedHeight;
                    float imageRatio = (float)texture.Width / texture.Height;
                    GameService.GetService<IRenderService>().DrawQuad(
                        texture,
                        new Rectangle(
                            RenderedX,
                            RenderedY,
                            (int)(elementRatio > imageRatio ? ((float)RenderedHeight / texture.Height * texture.Width) : RenderedWidth),
                            (int)(elementRatio > imageRatio ? RenderedHeight : ((float)RenderedWidth / texture.Width * texture.Height))
                        ),
                        new Rectangle(0, 0, texture.Width, texture.Height)
                    );
                    break;
                case Scaling.Window:
                    GameService.GetService<IRenderService>().DrawQuad(
                        texture,
                        new Rectangle(RenderedX, RenderedY, RenderedWidth, RenderedHeight),
                        new Rectangle((int)windowOffset.X, (int)windowOffset.Y, RenderedWidth, RenderedHeight)
                    );
                    break;
            }
        }
    }
}
