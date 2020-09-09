using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;

namespace WarnerEngine.Services.Implementations.Test
{
    public class TestContentService : IContentService
    {
        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize() {}

        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public TextureMetadata GetTextureMetadata(string Key)
        {
            throw new System.NotImplementedException();
        }

        public TextureMetadata GetTextureMetadata(Texture2D Key)
        {
            throw new System.NotImplementedException();
        }

        public IContentService Bootstrap(ContentManager CM, GraphicsDevice GD)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadAllContent()
        {
            throw new System.NotImplementedException();
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IContentService);
        }

        public Texture2D GetAtlasTexture()
        {
            throw new NotImplementedException();
        }

        public IContentService RegisterAssetLoader<TAsset>(Func<string, string, (string, object)[]> Loader, string ContentType)
        {
            throw new NotImplementedException();
        }

        public IContentService LoadAsset<TAsset>(string Path)
        {
            throw new NotImplementedException();
        }

        public IContentService LoadKeyedAsset<TAsset>(string Key, string Path)
        {
            throw new NotImplementedException();
        }

        public TAsset GetAsset<TAsset>(string Key) where TAsset : class
        {
            return null;
        }

        public TAsset GetxAsset<TAsset>(string Key) where TAsset : class
        {
            return null;
        }

        public IContentService SetRootDirectory(string RootDirectory)
        {
            return this;
        }

        public Texture2D GetWhiteTileTexture()
        {
            return null;
        }
    }
}
