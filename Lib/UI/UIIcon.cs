using System;

using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Helpers;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.UI
{
    public class UIIcon : UIElement<UIIcon>
    {
        private UIIconInfo iconInfo;

        public UIIcon(string Key, UIRenderer UIRendererInstance) : base(Key, UIRendererInstance) { }

        public UIIcon SetIcon(UIIconInfo IconInfo)
        {
            CheckCanModify();
            iconInfo = IconInfo;
            return this;
        }

        protected override UIIcon FinalizeImplementation()
        {
            return this;
        }

        public override void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY)
        {
            GameService.GetService<IRenderService>().DrawQuad(
                iconInfo.IconTextureKey,
                new Rectangle(RenderedX, RenderedY, iconInfo.IconWidth, iconInfo.IconHeight),
                GraphicsHelper.GetSheetCell(iconInfo.IconIndex, iconInfo.IconWidth, iconInfo.IconHeight)
            );
        }
    }
}
