using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Content;
using WarnerEngine.Lib.Dialog;

namespace WarnerEngine.Services
{
    public class ContentService : IContentService
    {
        public const string ATLAS_TARGET_KEY = "content_atlas";
        public const int ATLAS_WIDTH = 2048;
        public const int ATLAS_HEIGHT = 2048;

        private ContentManager contentManager;
        private GraphicsDevice graphicsDevice;

        private Dictionary<Type, Func<string, string, (string, object)[]>> assetLoaders;
        private Dictionary<string, Type> contentTypesToAssetTypes;
        private Dictionary<Type, Dictionary<string, object>> assets;

        private TextureAtlas textureAtlas;
        private bool hasBakedTextureAtlas;
        private Texture2D atlasTexture;

        public ContentService()
        {
            assetLoaders = new Dictionary<Type, Func<string, string, (string, object)[]>>();
            contentTypesToAssetTypes = new Dictionary<string, Type>();
            RegisterAssetLoader<Texture2D>(LoadTexture, "Texture");
            RegisterAssetLoader<Animation>(LoadAnimation, "Animation");
            RegisterAssetLoader<Effect>(LoadEffect, "Effect");
            RegisterAssetLoader<DialogLink>(LoadDialogLinks, "Dialog");
            RegisterAssetLoader<SoundEffect>(LoadSoundEffect, "Sound");
            RegisterAssetLoader<SpriteFont>(LoadSpriteFont, "SpriteFont");

            assets = new Dictionary<Type, Dictionary<string, object>>();

            textureAtlas = new TextureAtlas(0, 0, ATLAS_WIDTH, ATLAS_HEIGHT);
            hasBakedTextureAtlas = false;

            GameService.GetService<EventService>().Subscribe(
                Events.GRAPHICS_DEVICE_INITIALIZED,
                _ =>
                {
                    RenderService renderService = GameService.GetService<RenderService>();
                    renderService
                        .AddRenderTarget(
                            ATLAS_TARGET_KEY,
                            ATLAS_WIDTH,
                            ATLAS_HEIGHT,
                            RenderTargetUsage.PreserveContents
                        );
                }
            );
        }

        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            if (hasBakedTextureAtlas)
            {
                return ServiceCompositionMetadata.Empty;
            }
            textureAtlas.BakeTextureAtlas(ATLAS_TARGET_KEY);
            atlasTexture = GameService.GetService<RenderService>().ConvertRenderTargetToTexture(ATLAS_TARGET_KEY);
            hasBakedTextureAtlas = true;
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public IContentService InitializeContentService(ContentManager CM, GraphicsDevice GD)
        {
            contentManager = CM;
            graphicsDevice = GD;
            return this;
        }

        public IContentService RegisterAssetLoader<TAsset>(Func<string, string, (string, object)[]> Loader, string ContentType)
        {
            assetLoaders[typeof(TAsset)] = Loader;
            contentTypesToAssetTypes[ContentType] = typeof(TAsset);
            return this;
        }

#if ANDROID
        private bool GetContentManifestsAndroid(string Path, List<string> RParam)
        {
            string[] list;
            list = Android.App.Application.Context.Assets.List(Path);
            if (list.Length > 0)
            {
                // This is a folder
                foreach (string file in list)
                {
                    string maybeFileName = Path + "/" + file;
                    if (!GetContentManifestsAndroid(maybeFileName, RParam))
                    {
                        return false;
                    }
                    else if (file == "content_manifest.xml")
                    {
                        RParam.Add(maybeFileName);
                    }
                }
            }

            return true;
        }
#endif

        public IContentService LoadAllContent()
        {
#if ANDROID
            List<string> manifests = new List<string>();
            GetContentManifestsAndroid("Content", manifests);
            string[] files = manifests.ToArray();
#else
            string[] files = Directory.GetFiles("Content/", "content_manifest.xml", SearchOption.AllDirectories);
#endif
            XmlSerializer s = new XmlSerializer(typeof(ContentItem[]));
            foreach (string file in files)
            {
#if ANDROID
                string manifestPath = file.Substring(file.IndexOf('/') + 1, file.LastIndexOf('/') - file.IndexOf('/'));
#else
                string manifestPath = file.Substring(file.IndexOf('/') + 1, file.LastIndexOf('\\') + 1 - (file.IndexOf('/') + 1));
#endif
                using (Stream fs = TitleContainer.OpenStream(file))
                {
                    ContentItem[] contentItems = (ContentItem[])s.Deserialize(fs);
                    foreach (ContentItem item in contentItems)
                    {
                        string filePath = manifestPath + item.Path;
                        switch (item.Type)
                        {
                            case "Animation":
                                LoadAsset<Animation>(filePath);
                                break;
                            case "Animation4":
                                LoadAllAnimationDirections(filePath);
                                break;
                            case "Texture":
                                LoadKeyedAsset<Texture2D>(item.Key, filePath);
                                break;
                            case "Sound":
                                LoadKeyedAsset<SoundEffect>(item.Key, filePath);
                                break;
                            case "SpriteFont":
                                LoadKeyedAsset<SpriteFont>(item.Key, filePath);
                                break;
                            case "Dialog":
                                LoadAsset<DialogLink>(filePath);
                                break;
                            case "Effect":
                                LoadKeyedAsset<Effect>(item.Key, filePath);
                                break;
                            default:
                                LoadKeyedAssetImplementation(item.Key, filePath, contentTypesToAssetTypes[item.Type]);
                                break;
                        }
                    }
                }
            }
            // Build the texture atlas from all of the textures loaded
            //textureAtlas.BulkAddTextures(assets[typeof(Texture2D)]);
            return this;
        }

        private (string, object)[] LoadTexture(string Key, string Resource)
        {
            Texture2D texture;
            using (Stream fs = TitleContainer.OpenStream("Content/" + Resource + ".png"))
            {
                texture = Texture2D.FromStream(graphicsDevice, fs);
            }
            return new (string, object)[] { (Key, texture) };
        }

        private (string, object)[] LoadAnimation(string _, string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(Animation));
            Animation animation;
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".xml"))
            {
                animation = (Animation)s.Deserialize(fs);
            }
            return new (string, object)[] { (animation.animationKey, animation) };
        }

        private IContentService LoadAllAnimationDirections(string Path)
        {
            foreach (string dir in Enum.GetNames(typeof(Enums.Direction)))
            {
                LoadAsset<Animation>(Path + "_" + dir);
            }
            return this;
        }

        private (string, object)[] LoadSpriteFont(string Key, string Path)
        {
            SpriteFont font = contentManager.Load<SpriteFont>(Path);
            return new (string, object)[] { (Key, font) };
        }

        private (string, object)[] LoadSoundEffect(string Key, string Path)
        {
            SoundEffect sound;
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".wav"))
            {
                sound = SoundEffect.FromStream(fs);
            }
            return new (string, object)[] { (Key, sound) };
        }

        private (string, object)[] LoadDialogLinks(string _, string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(DialogLink[]));
            List<(string, object)> dialogLinks = new List<(string, object)>();
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".xml"))
            {
                DialogLink[] loadedDialog = (DialogLink[])s.Deserialize(fs);
                foreach (DialogLink d in loadedDialog)
                {
                    dialogLinks.Add((d.Key, d));
                }
            }
            return dialogLinks.ToArray();
        }

        private (string, object)[] LoadEffect(string Key, string Path)
        {
            Effect effect = contentManager.Load<Effect>(Path);
            return new (string, object)[] { (Key, effect) };
        }

        public IContentService LoadAsset<TAsset>(string Path)
        {
            return LoadKeyedAsset<TAsset>(null, Path);
        }

        public IContentService LoadKeyedAsset<TAsset>(string Key, string Path)
        {
            return LoadKeyedAssetImplementation(Key, Path, typeof(TAsset));
        }

        public IContentService LoadKeyedAssetImplementation(string Key, string Path, Type AssetType)
        {
            if (!assetLoaders.ContainsKey(AssetType))
            {
                throw new Exception("No loader present for asset type");
            }
            (string, object)[] loadedAssets = assetLoaders[AssetType](Key, Path);
            if (!assets.ContainsKey(AssetType))
            {
                assets[AssetType] = new Dictionary<string, object>();
            }
            foreach ((string key, object asset) in loadedAssets)
            {
                assets[AssetType][key] = asset;
            }
            return this;
        }

        public Texture2D GetTexture(string Key)
        {
            return GetAsset<Texture2D>(Key);
        }

        public TextureMetadata GetTextureMetadata(string Key)
        {
            if (!textureAtlas.StringKeyedTextureInfo.ContainsKey(Key))
            {
                return null;
            }
            return textureAtlas.StringKeyedTextureInfo[Key];
        }

        public TextureMetadata GetTextureMetadata(Texture2D Key)
        {
            if (!textureAtlas.TextureKeyedTextureInfo.ContainsKey(Key))
            {
                return null;
            }
            return textureAtlas.TextureKeyedTextureInfo[Key];
        }

        public Animation GetxAnimation(string Key)
        {
            return GetAsset<Animation>(Key);
        }

        public Animation GetAnimation(string Key)
        {
            if (Key == null || !assets.ContainsKey(typeof(Animation)) || !assets[typeof(Animation)].ContainsKey(Key))
            {
                return null;
            }
            return GetxAnimation(Key);
        }

        public SpriteFont GetSpriteFont(string Key)
        {
            return GetAsset<SpriteFont>(Key);
        }

        public SoundEffect GetSoundEffect(string Key)
        {
            return GetAsset<SoundEffect>(Key);
        }

        public DialogLink GetDialogLink(string Key)
        {
            return GetAsset<DialogLink>(Key);
        }

        public Effect GetEffect(string Key)
        {
            return GetAsset<Effect>(Key);
        }

        public TAsset GetAsset<TAsset>(string Key)
        {
            return (TAsset)assets[typeof(TAsset)][Key];
        }

        public Texture2D GetAtlasTexture()
        {
            return atlasTexture;
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IContentService);
        }
    }
}
