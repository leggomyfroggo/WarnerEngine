using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Helpers;

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
        public readonly bool IsScrollable;

        public UIDrawCall(UIDrawCall[] ChildDrawCalls, IUIElement Element, int Width, int Height, int X, int Y, bool IsScrollable)
        {
            this.ChildDrawCalls = ChildDrawCalls;
            this.Element = Element;
            this.Width = Width;
            this.Height = Height;
            this.X = X;
            this.Y = Y;
            this.IsScrollable = IsScrollable;
        }

        public bool PreDraw(float DT, bool AreMouseEventsBlocked, string FocusedElementKey, Vector2? MaybeCursorPosition)
        {
            for (int i = ChildDrawCalls.Length - 1; i >= 0; i--)
            {
                UIDrawCall child = ChildDrawCalls[i];
                AreMouseEventsBlocked = child.PreDraw(
                    DT, 
                    AreMouseEventsBlocked, 
                    FocusedElementKey,
                    ContainsPoint(MaybeCursorPosition) ? MaybeCursorPosition : null
                );
            }
            return Element.PreDrawBase(
                DT, 
                this, 
                AreMouseEventsBlocked, 
                FocusedElementKey == Element.GetKey(), 
                MaybeCursorPosition
            );
        }

        public void Draw(string FocusedElementKey, UIRenderer UIRendererInstance)
        {
            if (IsScrollable)
            {
                int innerHeight = 0;
                foreach (UIDrawCall child in ChildDrawCalls)
                {
                    innerHeight += child.Height + child.Element.GetHeight().FullMargin;
                }
                int scroll = MathHelper.Clamp(Element.GetScroll(), 0, innerHeight - Height);
                if (scroll != Element.GetScroll())
                {
                    Element.SetScroll(scroll);
                }
                UIRendererInstance.EnableScrollingTarget(this);
            }
            Element.Draw(Width, Height, X, Y, Element.GetKey() == FocusedElementKey);
            foreach (UIDrawCall child in ChildDrawCalls)
            {
                child.Draw(FocusedElementKey, UIRendererInstance);
            }
            if (IsScrollable)
            {
                UIRendererInstance.FlipScrollingTarget(this);
            }
        }

        public bool ContainsPoint(Vector2? MaybePoint) 
        {
            if (MaybePoint == null)
            {
                return false;
            }
            Vector2 P = MaybePoint.Value;
            return P.X >= X && P.X <= X + Width && P.Y >= Y && P.Y <= Y + Height;
        }
    }
}
