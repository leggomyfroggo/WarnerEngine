using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;

namespace WarnerEngine.Lib.Entities
{
    public interface IShadowCaster
    {
        Box GetShadowVolume();
        Texture2D GetShadowTexture();
        float GetShadowOpacity();
        bool ShouldTile();
    }
}
