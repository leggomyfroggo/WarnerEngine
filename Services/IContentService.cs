using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public interface IContentService : IService
    {
        IContentService Bootstrap(ContentManager CM, GraphicsDevice GD);

        IContentService SetLocale(Enums.LocaleCode Locale);

        IContentService SetRootDirectory(string RootDirectory);

        IContentService RegisterAssetLoader<TAsset>(Func<string, string, (string, object)[]> Loader, string ContentType);

        IContentService LoadAllContent();

        IContentService LoadAsset<TAsset>(string Path);
        IContentService LoadKeyedAsset<TAsset>(string Key, string Path);

        TextureMetadata GetTextureMetadata(string Key);
        TextureMetadata GetTextureMetadata(Texture2D Key);

        TAsset GetAsset<TAsset>(string Key) where TAsset : class;
        TAsset GetxAsset<TAsset>(string Key) where TAsset : class;

        Texture2D GetAtlasTexture();

        Texture2D GetWhiteTileTexture();
    }
}
