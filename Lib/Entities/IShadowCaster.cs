using Microsoft.Xna.Framework.Graphics;

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
