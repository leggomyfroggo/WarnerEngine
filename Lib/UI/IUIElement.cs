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
        void PreDrawBase(float DT, UIDrawCall DrawCall);
        List<UIDrawCall> RenderAsRoot();
        List<UIDrawCall> Render(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY);
        void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY);
        Dictionary<string, object> GetDefaultState();
    }
}
