using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib.UI
{
    public interface IUIElement
    {
        string GetKey();
        UISize GetWidth();
        UISize GetHeight();
        int GetX();
        int GetY();
        bool IsScrollingEnabled();
        int GetScroll();
        void SetScroll(int Scroll);
        UIEnums.Positioning GetPositioning();
        UIEnums.ChildOrdering GetChildOrdering();
        bool PreDrawBase(float DT, UIDrawCall DrawCall, bool AreMouseEventsBlocked, bool IsFocused, Vector2? MaybeCursorPosition);
        UIDrawCall RenderAsRoot();
        UIDrawCall Render(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY);
        void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY, bool IsFocused);
        Dictionary<string, object> GetDefaultState();
    }
}
