using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Content;
using WarnerEngine.Lib.Dialog;

namespace WarnerEngine.Services.Implementations
{
    public class ContentService : IContentService
    {
        public const string ATLAS_TARGET_KEY = "content_atlas";
        public const int ATLAS_WIDTH = 2048;
        public const int ATLAS_HEIGHT = 2048;

        public const string DEFAULT_ROOT_DIRECTORY = "Content/";
        private const string ENGINE_ROOT_DIRECTORY = "__Content__/";

        private const string WHITE_TILE_TEXTURE_KEY = "ENGINE_white_tile";

        private ContentManager contentManager;
        private GraphicsDevice graphicsDevice;
        private string rootDirectory;

        private Dictionary<Type, Func<string, string, (string, object)[]>> assetLoaders;
        private Dictionary<string, Type> contentTypesToAssetTypes;
        private Dictionary<Type, Dictionary<string, object>> assets;

        private TextureAtlas textureAtlas;
        private bool hasBakedTextureAtlas;
        private Texture2D atlasTexture;

        private Enums.LocaleCode localeCode;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>()
            {
                typeof(IEventService),
            };
        }

        public void Initialize()
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

            localeCode = Enums.LocaleCode.en_US;

            GameService.GetService<IEventService>().Subscribe(
                Events.GRAPHICS_DEVICE_INITIALIZED,
                _ =>
                {
                    IRenderService renderService = GameService.GetService<IRenderService>();
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
            atlasTexture = GameService.GetService<IRenderService>().ConvertRenderTargetToTexture(ATLAS_TARGET_KEY);
            hasBakedTextureAtlas = true;
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public IContentService Bootstrap(ContentManager CM, GraphicsDevice GD)
        {
            contentManager = CM;
            graphicsDevice = GD;

            // Temporarily switch to the engine assets folder
            SetRootDirectory(ENGINE_ROOT_DIRECTORY);

            // Load content that's used by the engine
            LoadKeyedAsset<Texture2D>(WHITE_TILE_TEXTURE_KEY, "whiteTile");

            // Switch back to the default root
            SetRootDirectory(DEFAULT_ROOT_DIRECTORY);

            return this;
        }

        public IContentService SetLocale(Enums.LocaleCode Locale)
        {
            localeCode = Locale;
            return this;
        }

        public IContentService SetRootDirectory(string RootDirectory)
        {
            rootDirectory = RootDirectory;
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
            List<ContentItem> contentItems = new List<ContentItem>();
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
                    ContentItem[] tempItems = (ContentItem[])s.Deserialize(fs);
                    for (int i = 0; i < tempItems.Length; i++)
                    {
                        tempItems[i].Path = Path.Combine(manifestPath, tempItems[i].Path);
                    }
                    contentItems.AddRange(tempItems);
                }
            }

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < 4; i++)
            {
                Func<int, Action> starter = (capturedIndex) => (() => LoadContentThread(contentItems, capturedIndex, 4));
                var thread = new Thread(new ThreadStart(starter(i)));
                thread.Start();
                threads.Add(thread);
            }
            foreach (Thread t in threads)
            {
                t.Join();
            }
            // Build the texture atlas from all of the textures loaded
            //textureAtlas.BulkAddTextures(assets[typeof(Texture2D)]);
            return this;
        }

        private void LoadContentThread(List<ContentItem> Items, int ThreadID, int ThreadCount)
        {
            for (int j = ThreadID; j < Items.Count; j += ThreadCount)
            {
                var item = Items[j];
                switch (item.Type)
                {
                    case "Animation":
                        LoadAsset<Animation>(item.Path);
                        break;
                    case "Animation4":
                        LoadAllAnimationDirections(item.Path);
                        break;
                    case "Texture":
                        LoadKeyedAsset<Texture2D>(item.Key, item.Path);
                        break;
                    case "Sound":
                        LoadKeyedAsset<SoundEffect>(item.Key, item.Path);
                        break;
                    case "SpriteFont":
                        LoadKeyedAsset<SpriteFont>(item.Key, item.Path);
                        break;
                    case "Dialog":
                        LoadAsset<DialogLink>(item.Path);
                        break;
                    case "Effect":
                        LoadKeyedAsset<Effect>(item.Key, item.Path);
                        break;
                    default:
                        LoadKeyedAssetImplementation(item.Key, item.Path, contentTypesToAssetTypes[item.Type]);
                        break;
                }
            }
        }

        private (string, object)[] LoadTexture(string Key, string Resource)
        {
            Texture2D texture;
            using (Stream fs = TitleContainer.OpenStream(Path.ChangeExtension(Path.Combine(rootDirectory, Resource), ".png")))
            {
                texture = Texture2D.FromStream(graphicsDevice, fs);
            }
            return new (string, object)[] { (Key, texture) };
        }

        private (string, object)[] LoadAnimation(string _, string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(Animation));
            Animation animation;
            using (Stream fs = TitleContainer.OpenStream(rootDirectory + Path + ".xml"))
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
            using (Stream fs = TitleContainer.OpenStream(rootDirectory + Path + ".wav"))
            {
                sound = SoundEffect.FromStream(fs);
            }
            return new (string, object)[] { (Key, sound) };
        }

        private (string, object)[] LoadDialogLinks(string _, string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(DialogLink[]));
            List<(string, object)> dialogLinks = new List<(string, object)>();
            using (Stream fs = TitleContainer.OpenStream(rootDirectory + Path + "." + localeCode + ".xml"))
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
                lock (assets)
                {
                    assets[AssetType] = new Dictionary<string, object>();
                }
            }
            foreach ((string key, object asset) in loadedAssets)
            {
                lock (assets) 
                {
                    assets[AssetType][key] = asset;
                }
            }
            return this;
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

        public TAsset GetAsset<TAsset>(string Key) where TAsset : class
        {
            if (Key == null || !assets.ContainsKey(typeof(TAsset)) || !assets[typeof(TAsset)].ContainsKey(Key))
            {
                return null;
            }
            return GetxAsset<TAsset>(Key);
        }

        public TAsset GetxAsset<TAsset>(string Key) where TAsset : class
        {
            return (TAsset)assets[typeof(TAsset)][Key];
        }

        public Texture2D GetAtlasTexture()
        {
            return atlasTexture;
        }

        public Texture2D GetWhiteTileTexture()
        {
            return GetAsset<Texture2D>(WHITE_TILE_TEXTURE_KEY);
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IContentService);
        }
    }
}
