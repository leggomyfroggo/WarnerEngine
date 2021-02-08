using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.UI
{
    public struct UIDrawCall
    {
        public readonly UIDrawCall[] ChildDrawCalls;
        public readonly IUIElement Element;
        public readonly int Width;
        public readonly int Height;
        public readonly int X;
        public readonly int Y;

        public UIDrawCall(UIDrawCall[] ChildDrawCalls, IUIElement Element, int Width, int Height, int X, int Y)
        {
            this.ChildDrawCalls = ChildDrawCalls;
            this.Element = Element;
            this.Width = Width;
            this.Height = Height;
            this.X = X;
            this.Y = Y;
        }

        public bool PreDraw(float DT, bool AreMouseEventsBlocked, string FocusedElementKey)
        {
            for (int i = ChildDrawCalls.Length - 1; i >= 0; i--)
            {
                UIDrawCall child = ChildDrawCalls[i];
                AreMouseEventsBlocked = child.PreDraw(DT, AreMouseEventsBlocked, FocusedElementKey);
            }
            return Element.PreDrawBase(DT, this, AreMouseEventsBlocked, FocusedElementKey == Element.GetKey());
        }

        public void Draw(string FocusedElementKey)
        {
            Element.Draw(Width, Height, X, Y, Element.GetKey() == FocusedElementKey);
            foreach (UIDrawCall child in ChildDrawCalls)
            {
                child.Draw(FocusedElementKey);
            }
        }

        public bool ContainsPoint(Vector2 P) 
        {
            return P.X >= X && P.X <= X + Width && P.Y >= Y && P.Y <= Y + Height;
        }
    }
}
