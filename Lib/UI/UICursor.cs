using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;
using WarnerEngine.Lib.Helpers;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.UI
{
    public class UICursor : ISceneEntity, IDraw
    {
        private bool shouldDrawDeferred;

        public UICursor(bool ShouldDrawDeferred = false)
        {
            shouldDrawDeferred = ShouldDrawDeferred;
        }

        public void OnAdd(Scene ParentScene) { }

        public void OnRemove(Scene ParentScene) { }

        public void Draw()
        {
            Vector2 cursorPosition = GameService.GetService<IInputService>().GetMouseInScreenSpace();
            if (shouldDrawDeferred)
            {
                GameService.GetService<IRenderService>().AddDeferredCall(_ => DrawCursor(cursorPosition));
            } 
            else
            {
                DrawCursor(cursorPosition);
            }
        }

        protected virtual void DrawCursor(Vector2 CursorPosition)
        {
            GraphicsHelper.DrawSquare((int)CursorPosition.X, (int)CursorPosition.Y, 2, 2, Color.White, true);
        }

        public BackingBox GetBackingBox()
        {
            return BackingBox.Dummy;
        }

        public bool IsVisible()
        {
            return true;
        }
    }
}
