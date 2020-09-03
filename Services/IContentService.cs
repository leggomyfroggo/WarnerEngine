using System;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Content;
using WarnerEngine.Lib.Dialog;

namespace WarnerEngine.Services
{
    public interface IContentService : IService
    {
        IContentService InitializeContentService(ContentManager CM, GraphicsDevice GD);

        IContentService RegisterAssetLoader<TAsset>(Func<string, string, (string, object)[]> Loader, string ContentType);

        IContentService LoadAllContent();

        IContentService LoadAsset<TAsset>(string Path);
        IContentService LoadKeyedAsset<TAsset>(string Key, string Path);

        TextureMetadata GetTextureMetadata(string Key);
        TextureMetadata GetTextureMetadata(Texture2D Key);

        TAsset GetAsset<TAsset>(string Key) where TAsset : class;
        TAsset GetxAsset<TAsset>(string Key);

        Texture2D GetAtlasTexture();
    }
}
