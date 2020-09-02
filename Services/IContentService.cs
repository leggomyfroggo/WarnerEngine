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

        IContentService LoadAllContent();
        IContentService LoadTexture(string Key, string Resource);
        IContentService LoadAnimation(string Path);
        IContentService LoadAllAnimationDirections(string Path);
        IContentService LoadSpriteFont(string Key, string Path);
        IContentService LoadSoundEffect(string Key, string Path);
        IContentService LoadDialog(string Path);
        IContentService LoadDialogLinks(string Path);
        IContentService LoadEffect(string Key, string Path);
        IContentService LoadWorldGroupDefinition(string Path);

        Texture2D GetTexture(string Key);
        TextureMetadata GetTextureMetadata(string Key);
        TextureMetadata GetTextureMetadata(Texture2D Key);
        Animation GetxAnimation(string Key);
        Animation GetAnimation(string Key);
        SpriteFont GetSpriteFont(string Key);
        SoundEffect GetSoundEffect(string Key);
        DialogChain GetDialog(string Key);
        DialogLink GetDialogLink(string Key);
        Effect GetEffect(string Key);
        ProjectWarnerShared.Scenes.WorldGroupDefinition GetWorldGroupDefinition(string Key);

        Texture2D GetAtlasTexture();
    }
}
