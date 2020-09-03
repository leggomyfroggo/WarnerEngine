using System;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Content;
using WarnerEngine.Lib.Dialog;

namespace WarnerEngine.Services
{
    public class TestContentService : IContentService
    {
        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public Animation GetAnimation(string Key)
        {
            throw new System.NotImplementedException();
        }

        public DialogLink GetDialogLink(string Key)
        {
            throw new System.NotImplementedException();
        }

        public Effect GetEffect(string Key)
        {
            throw new System.NotImplementedException();
        }

        public SoundEffect GetSoundEffect(string Key)
        {
            return null;
        }

        public SpriteFont GetSpriteFont(string Key)
        {
            throw new System.NotImplementedException();
        }

        public TextureMetadata GetTextureMetadata(string Key)
        {
            throw new System.NotImplementedException();
        }

        public TextureMetadata GetTextureMetadata(Texture2D Key)
        {
            throw new System.NotImplementedException();
        }

        public Animation GetxAnimation(string Key)
        {
            throw new System.NotImplementedException();
        }

        public IContentService InitializeContentService(ContentManager CM, GraphicsDevice GD)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadAllAnimationDirections(string Path)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadAllContent()
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadAnimation(string Path)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadDialog(string Path)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadDialogLinks(string Path)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadEffect(string Key, string Path)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadSoundEffect(string Key, string Path)
        {
            throw new System.NotImplementedException();
        }

        public IContentService LoadSpriteFont(string Key, string Path)
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

        public TAsset GetAsset<TAsset>(string Key)
        {
            throw new NotImplementedException();
        }

        public Texture2D GetTexture(string Key)
        {
            throw new NotImplementedException();
        }
    }
}
