using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ProjectWarnerShared.Lib.UI
{
    public class UIButton : UIElement<UIButton>
    {
        private enum State { Neutral, Hover, Pressed };

        private Color color;
        private Color hoverColor;
        private Color pressColor;
        private UIIconInfo iconInfo;

        public UIButton(string Key, UIRenderer UIRendererInstance) : base(Key, UIRendererInstance) { }

        public UIButton SetColor(Color C)
        {
            CheckCanModify();
            color = C;
            return this;
        }

        public UIButton SetHoverColor(Color C)
        {
            CheckCanModify();
            hoverColor = C;
            return this;
        }

        public UIButton SetPressColor(Color C)
        {
            CheckCanModify();
            pressColor = C;
            return this;
        }

        public UIButton SetIcon(UIIconInfo IconInfo)
        {
            CheckCanModify();
            iconInfo = IconInfo;
            return this;
        }

        protected override UIButton FinalizeImplementation()
        {
            return SetChildren(
                new UIBox(key + "_box", uiRendererInstance)
                    .SetWidth(new UISize(GetWidth().Sizing, GetWidth().Size))
                    .SetHeight(new UISize(GetHeight().Sizing, GetHeight().Size))
                    .SetColor(GetColorForCurrentState())
                    .SetOnClick(onClick)
                    .SetOnEnter(_ => SetState("state", State.Hover))
                    .SetOnExit(_ => SetState("state", State.Neutral))
                    .SetOnPress(_ => SetState("state", State.Pressed))
                    .SetOnRelease(_ => SetState("state", State.Neutral))
                    .SetChildren(
                        new UIIcon(key + "_icon", uiRendererInstance)
                            .SetIcon(iconInfo)
                            .Finalize()
                    )
                    .Finalize()
            );
        }

        private Color GetColorForCurrentState()
        {
            State currentState = GetState<State>("state");
            switch (currentState)
            {
                case State.Neutral:
                    return color;
                case State.Hover:
                    return hoverColor;
                case State.Pressed:
                    return pressColor;
            }
            return Color.Transparent;
        }

        public override void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY)
        {
        }

        public override Dictionary<string, object> GetDefaultState()
        {
            return new Dictionary<string, object>
            {
                { "state", State.Neutral },
            };
        }
    }
}
