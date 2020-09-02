using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectWarnerShared.Services;

namespace ProjectWarnerShared.Lib
{
    public class TextureAtlas
    {
        private List<TextureAtlas> subAtlases;

        public Dictionary<string, TextureMetadata> StringKeyedTextureInfo { get; private set; }
        public Dictionary<Texture2D, TextureMetadata> TextureKeyedTextureInfo { get; private set; }

        public bool IsLocked;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TextureAtlas(int X, int Y, int Width, int Height, bool IsLocked = false)
        {
            subAtlases = new List<TextureAtlas>();
            StringKeyedTextureInfo = new Dictionary<string, TextureMetadata>();
            TextureKeyedTextureInfo = new Dictionary<Texture2D, TextureMetadata>();
            this.IsLocked = IsLocked;
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public void BulkAddTextures(Dictionary<string, Texture2D> Textures)
        {
            var entries = Textures.Select(x => x).OrderByDescending(x => x.Value.Width * x.Value.Height);
            foreach (var entry in entries)
            {
                var result = AddTexture(entry.Key, entry.Value);
                if (result == null)
                {
                    throw new Exception("Could not insert texture into atlas");
                }
            }
        }

        public TextureMetadata AddTexture(string Key, Texture2D Texture)
        {
            if (IsLocked || Width < Texture.Width || Height < Texture.Height)
            {
                return null;
            }
            else if (subAtlases.Count == 0)
            {
                subAtlases.Add(new TextureAtlas(X, Y, Texture.Width, Texture.Height, true));
                var excessWidth = Width - Texture.Width;
                if (excessWidth > 0)
                {
                    subAtlases.Add(new TextureAtlas(X + Texture.Width, Y, excessWidth, Texture.Height));
                }
                var excessHeight = Height - Texture.Height;
                if (excessHeight > 0)
                {
                    subAtlases.Add(new TextureAtlas(X, Y + Texture.Height, Texture.Width, excessHeight));
                }
                if (excessWidth > 0 && excessHeight > 0)
                {
                    subAtlases.Add(new TextureAtlas(X + Texture.Width, Y + Texture.Height, excessWidth, excessHeight));
                }
                var newTextureInfo = new TextureMetadata(
                    Key,
                    X,
                    Y,
                    Texture.Width,
                    Texture.Height
                );
                StringKeyedTextureInfo[Key] = newTextureInfo;
                TextureKeyedTextureInfo[Texture] = newTextureInfo;
                return newTextureInfo;
            }
            else
            {
                foreach (TextureAtlas atlas in subAtlases)
                {
                    var maybeMetadata = atlas.AddTexture(Key, Texture);
                    if (maybeMetadata != null)
                    {
                        StringKeyedTextureInfo[Key] = maybeMetadata;
                        TextureKeyedTextureInfo[Texture] = maybeMetadata;
                        return maybeMetadata;
                    }
                }
            }
            IsLocked = true;
            return null;
        }

        public void BakeTextureAtlas(string RenderTargetKey)
        {
            var renderService = GameService.GetService<RenderService>();
            renderService
                .SetRenderTarget(RenderTargetKey)
                .Start();
            foreach (KeyValuePair<Texture2D, TextureMetadata> texToMetadata in TextureKeyedTextureInfo)
            {
                var texture = texToMetadata.Key;
                var metadata = texToMetadata.Value;
                renderService.DrawQuad(
                    texture,
                    new Rectangle(metadata.X, metadata.Y, metadata.Width, metadata.Height),
                    new Rectangle(0, 0, metadata.Width, metadata.Height)
                );
            }
            renderService.End();
        }
    }
}
