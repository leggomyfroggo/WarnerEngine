using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectWarnerShared.Lib;
using ProjectWarnerShared.Lib.Content;
using ProjectWarnerShared.Lib.Dialog;
using ProjectWarnerShared.Scenes;

namespace ProjectWarnerShared.Services
{
    public class ContentService : IContentService
    {
        public const string ATLAS_TARGET_KEY = "content_atlas";
        public const int ATLAS_WIDTH = 2048;
        public const int ATLAS_HEIGHT = 2048;

        private ContentManager contentManager;
        private GraphicsDevice graphicsDevice;
        private Dictionary<string, Texture2D> textures;
        private Dictionary<string, Animation> animations;
        private Dictionary<string, SpriteFont> spriteFonts;
        private Dictionary<string, SoundEffect> soundEffects;
        private Dictionary<string, DialogChain> dialog;
        private Dictionary<string, DialogLink> dialogLinks;
        private Dictionary<string, Effect> effects;
        private Dictionary<string, WorldGroupDefinition> worldGroupDefinitions;

        private TextureAtlas textureAtlas;
        private bool hasBakedTextureAtlas;
        private Texture2D atlasTexture;

        public ContentService()
        {
            textures = new Dictionary<string, Texture2D>();
            animations = new Dictionary<string, Animation>();
            spriteFonts = new Dictionary<string, SpriteFont>();
            soundEffects = new Dictionary<string, SoundEffect>();
            dialog = new Dictionary<string, DialogChain>();
            dialogLinks = new Dictionary<string, DialogLink>();
            effects = new Dictionary<string, Effect>();
            worldGroupDefinitions = new Dictionary<string, WorldGroupDefinition>();

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
                            case ContentItem.ContentType.Animation:
                                LoadAnimation(filePath);
                                break;
                            case ContentItem.ContentType.Animation4:
                                LoadAllAnimationDirections(filePath);
                                break;
                            case ContentItem.ContentType.Texture:
                                LoadTexture(item.Key, filePath);
                                break;
                            case ContentItem.ContentType.Sound:
                                LoadSoundEffect(item.Key, filePath);
                                break;
                            case ContentItem.ContentType.SpriteFont:
                                LoadSpriteFont(item.Key, filePath);
                                break;
                            case ContentItem.ContentType.Dialog:
                                LoadDialogLinks(filePath);
                                break;
                            case ContentItem.ContentType.Effect:
                                LoadEffect(item.Key, filePath);
                                break;
                            case ContentItem.ContentType.WorldGroupDefinition:
                                LoadWorldGroupDefinition(filePath);
                                break;
                        }
                    }
                }
            }
            // Build the texture atlas from all of the textures loaded
            textureAtlas.BulkAddTextures(textures);
            return this;
        }

        public IContentService LoadTexture(string Key, string Resource)
        {
            using (Stream fs = TitleContainer.OpenStream("Content/" + Resource + ".png"))
            {
                textures[Key] = Texture2D.FromStream(graphicsDevice, fs);
            }
            return this;
        }

        public IContentService LoadAnimation(string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(Animation));
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".xml"))
            {
                Animation animation = (Animation)s.Deserialize(fs);
                animations[animation.animationKey] = animation;
            }
            return this;
        }

        public IContentService LoadAllAnimationDirections(string Path)
        {
            foreach (string dir in Enum.GetNames(typeof(Enums.Direction)))
            {
                LoadAnimation(Path + "_" + dir);
            }
            return this;
        }

        public IContentService LoadSpriteFont(string Key, string Path)
        {
            spriteFonts[Key] = contentManager.Load<SpriteFont>(Path);
            return this;
        }

        public IContentService LoadSoundEffect(string Key, string Path)
        {
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".wav"))
            {
                soundEffects[Key] = SoundEffect.FromStream(fs);
            }
            return this;
        }

        public IContentService LoadDialog(string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(DialogChain[]));
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".xml"))
            {
                DialogChain[] loadedDialog = (DialogChain[])s.Deserialize(fs);
                foreach (DialogChain d in loadedDialog)
                {
                    dialog[d.DialogKey] = d;
                }
            }
            return this;
        }

        public IContentService LoadDialogLinks(string Path)
        {
            XmlSerializer s = new XmlSerializer(typeof(DialogLink[]));
            using (Stream fs = TitleContainer.OpenStream("Content/" + Path + ".xml"))
            {
                DialogLink[] loadedDialog = (DialogLink[])s.Deserialize(fs);
                foreach (DialogLink d in loadedDialog)
                {
                    dialogLinks[d.Key] = d;
                }
            }
            return this;
        }

        public IContentService LoadEffect(string Key, string Path)
        {
            effects[Key] = contentManager.Load<Effect>(Path);
            return this;
        }

        public IContentService LoadWorldGroupDefinition(string Path)
        {
            var worldGroupDefinition = WorldGroupDefinition.LoadFromFile("Content/" + Path + ".xml");
            worldGroupDefinitions[worldGroupDefinition.GroupKey] = worldGroupDefinition;
            return this;
        }

        public Texture2D GetTexture(string Key)
        {
            return textures[Key];
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
            return animations[Key];
        }

        public Animation GetAnimation(string Key)
        {
            if (Key == null || !animations.ContainsKey(Key))
            {
                return null;
            }
            return GetxAnimation(Key);
        }

        public SpriteFont GetSpriteFont(string Key)
        {
            return spriteFonts[Key];
        }

        public SoundEffect GetSoundEffect(string Key)
        {
            return soundEffects[Key];
        }

        public DialogChain GetDialog(string Key)
        {
            return dialog[Key];
        }

        public DialogLink GetDialogLink(string Key)
        {
            return dialogLinks[Key];
        }

        public Effect GetEffect(string Key)
        {
            return effects[Key];
        }

        public WorldGroupDefinition GetWorldGroupDefinition(string Key)
        {
            return worldGroupDefinitions[Key];
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
