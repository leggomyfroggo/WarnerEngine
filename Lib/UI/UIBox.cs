using Microsoft.Xna.Framework;
using WarnerEngine.Lib.Helpers;

namespace WarnerEngine.Lib.UI
{
    public class UIBox : UIElement<UIBox>
    {
        private Color tint;
        private bool isFilled;

        public UIBox(string Key, UIRenderer UIRendererInstance) : base(Key, UIRendererInstance) 
        {
            tint = Color.White;
            isFilled = true;
        }

        public UIBox SetColor(Color Tint)
        {
            tint = Tint;
            return this;
        }

        public UIBox SetIsFilled(bool IsFilled)
        {
            isFilled = IsFilled;
            return this;
        }

        protected override UIBox FinalizeImplementation() 
        {
            return this;
        }

        public override void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY, bool IsFocused)
        {
            if (tint == Color.Transparent)
            {
                return;
            }
            GraphicsHelper.DrawSquare(RenderedX, RenderedY, RenderedWidth, RenderedHeight, tint, isFilled);
        }
    }
}
