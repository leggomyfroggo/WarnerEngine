using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarnerEngine.Lib.UI
{
    public class UILeftRight : UIElement<UILeftRight>
    {
        private IUIElement left;
        private IUIElement right;

        public UILeftRight(string Key, UIRenderer UIRendererInstance) : base(Key, UIRendererInstance) { }

        public UILeftRight SetLeft(IUIElement Left)
        {
            left = Left;
            return this;
        }

        public UILeftRight SetRight(IUIElement Right)
        {
            right = Right;
            return this;
        }

        protected override UILeftRight FinalizeImplementation()
        {
            return SetChildren(
                left,
                new UIBox("stateless_ui_left_right_middle", uiRendererInstance)
                    .SetWidth(new UISize(UIEnums.Sizing.Rest, 100))
                    .SetHeight(new UISize(UIEnums.Sizing.Relative, 100))
                    .SetColor(Color.Transparent)
                    .Finalize(),
                right
            );
        }

        public override void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY, bool IsFocused) { }
    }
}
