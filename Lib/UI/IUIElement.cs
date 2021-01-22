using WarnerEngine.Lib.Entities;
using System.Collections.Generic;

namespace WarnerEngine.Lib.UI
{
    public interface IUIElement
    {
        string GetKey();
        UISize GetWidth();
        UISize GetHeight();
        int GetX();
        int GetY();
        UIEnums.Positioning GetPositioning();
        UIEnums.ChildOrdering GetChildOrdering();
        bool PreDrawBase(float DT, UIDrawCall DrawCall, bool AreMouseEventsBlocked, bool IsFocused);
        List<UIDrawCall> RenderAsRoot();
        List<UIDrawCall> Render(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY);
        void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY, bool IsFocused);
        Dictionary<string, object> GetDefaultState();
    }
}
