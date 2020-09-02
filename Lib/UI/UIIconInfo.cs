using ProjectWarnerShared.Lib.Helpers;

namespace ProjectWarnerShared.Lib.UI
{
    public struct UIIconInfo
    {
        public readonly string IconTextureKey;
        public readonly Index2 IconIndex;
        public readonly int IconWidth;
        public readonly int IconHeight;

        public UIIconInfo(string IconTextureKey, Index2 IconIndex, int IconWidth, int IconHeight)
        {
            this.IconTextureKey = IconTextureKey;
            this.IconIndex = IconIndex;
            this.IconWidth = IconWidth;
            this.IconHeight = IconHeight;
        }
    }
}
