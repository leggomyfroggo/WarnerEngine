using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.UI
{
    public struct UIDrawCall : IPreDraw
    {
        public readonly IUIElement Element;
        public readonly int Width;
        public readonly int Height;
        public readonly int X;
        public readonly int Y;

        public UIDrawCall(IUIElement Element, int Width, int Height, int X, int Y)
        {
            this.Element = Element;
            this.Width = Width;
            this.Height = Height;
            this.X = X;
            this.Y = Y;
        }

        public void PreDraw(float DT)
        {
            Element.PreDrawBase(DT, this);
        }

        public void Draw()
        {
            Element.Draw(Width, Height, X, Y);
        }

        public bool ContainsPoint(Vector2 P) 
        {
            return P.X >= X && P.X <= X + Width && P.Y >= Y && P.Y <= Y + Height;
        }
    }
}
