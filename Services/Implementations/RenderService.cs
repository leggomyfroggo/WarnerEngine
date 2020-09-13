using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Dialog;
using WarnerEngine.Lib.Helpers;
using WarnerEngine.Lib.Structure.Shadows;

namespace WarnerEngine.Services.Implementations
{
    public class RenderService : IRenderService
    {
        public static class AlphaStacks
        {
            public const string Shadows = "overworld_ss";
            public const string WaterStatic = "overworld_ws";
            public const string WaterDynamic = "overworld_dws";
            public const string CoveredItems = "overworld_cis";
            public const string TransparentItems = "overworld_tis";
        }

        private static class AlphaStackSizes
        {
            public const int Shadows = 500;
            public const int WaterStatic = 500;
            public const int WaterDynamic = 50;
            public const int CoveredItems = 50;
            public const int TransparentItems = 10000;
        }

        public const string FINAL_TARGET_KEY = "real_final_composite_target";

        public int InternalResolutionX { get; private set; }
        public int InternalResolutionY { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }
        private SpriteBatch spriteBatch;
        private SpriteSortMode sortMode;
        private BlendState blendState;
        private SamplerState samplerState;
        private DepthStencilState depthStencilState;
        private Vector2 scroll;
        public bool IsDrawing { get; private set; }
        private Effect currentEffect;

        private float depth;

        private Dictionary<string, RenderTarget2D> targets;
        private RenderTarget2D currentTarget;

        private List<Action<SpriteBatch>> deferredCalls;
        private List<Action> worldLockedDeferredCalls;

        private Dictionary<string, AlphaStack> alphaStacks;

        public int ResolutionX => GraphicsDevice.PresentationParameters.BackBufferWidth;
        public int ResolutionY => GraphicsDevice.PresentationParameters.BackBufferHeight;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize()
        {
            targets = new Dictionary<string, RenderTarget2D>();
            IsDrawing = false;
            deferredCalls = new List<Action<SpriteBatch>>();
            worldLockedDeferredCalls = new List<Action>();
            alphaStacks = new Dictionary<string, AlphaStack>();
            this
                .BuildAlphaStack(AlphaStacks.Shadows, AlphaStackSizes.Shadows)
                .BuildAlphaStack(AlphaStacks.WaterStatic, AlphaStackSizes.WaterStatic)
                .BuildAlphaStack(AlphaStacks.WaterDynamic, AlphaStackSizes.WaterDynamic)
                .BuildAlphaStack(AlphaStacks.CoveredItems, AlphaStackSizes.CoveredItems)
                .BuildAlphaStack(AlphaStacks.TransparentItems, AlphaStackSizes.TransparentItems);
        }

        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public IRenderService SetGraphicsDevice(GraphicsDevice GD)
        {
            GraphicsDevice = GD;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameService.GetService<IEventService>().Notify(Events.GRAPHICS_DEVICE_INITIALIZED);
            return this;
        }

        public IRenderService SetInternalResolution(int X, int Y)
        {
            InternalResolutionX = X;
            InternalResolutionY = Y;
            GameService.GetService<IEventService>().Notify(Events.INTERNAL_RESOLUTION_CHANGED);
            AddRenderTarget(FINAL_TARGET_KEY, InternalResolutionX, InternalResolutionY, RenderTargetUsage.PreserveContents);
            return this;
        }

        public IRenderService ConditionallyRunPipelineBlock(Func<IRenderService, bool> ConditionFunction, Func<IRenderService, IRenderService> PipelineBlock)
        {
            if (ConditionFunction(this))
            {
                return PipelineBlock(this);
            }
            return this;
        }

        public IRenderService BuildAlphaStack(string Key, int Size)
        {
            alphaStacks[Key] = new AlphaStack(Size);
            return this;
        }

        public IRenderService PushAlphaFragment(string Key, Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity = 1f, float Rotation = 0f, Vector2? Origin = null, Color? Tint = null, bool IsTiling = false)
        {
            alphaStacks[Key].PushFragment(
                Texture,
                DestinationRectangle,
                SourceRectangle,
                Opacity,
                depth,
                Rotation: Rotation,
                Origin: Origin,
                Tint: Tint,
                IsTiling: IsTiling
            );
            return this;
        }

        public IRenderService DrawAlphaStack(string Key, Color? Tint = null)
        {
            alphaStacks[Key].Draw(Tint);
            return this;
        }

        public IRenderService FlushAlphaStack(string Key, Color? Tint = null)
        {
            DrawAlphaStack(Key, Tint);
            alphaStacks[Key].ClearStack();
            return this;
        }

        public int GetAlphaStackFragmentCount(string Key)
        {
            return alphaStacks[Key].TotalFragments;
        }

        public IRenderService SetSortMode(SpriteSortMode SortMode)
        {
            sortMode = SortMode;
            return this;
        }

        public IRenderService SetBlendState(BlendState BlendState)
        {
            blendState = BlendState;
            return this;
        }

        public IRenderService SetSamplerState(SamplerState SS)
        {
            samplerState = SS;
            return this;
        }

        public IRenderService SetDepthStencilState(DepthStencilState DSS)
        {
            depthStencilState = DSS;
            return this;
        }


        public IRenderService Start(Vector2? Scroll = null)
        {
            Matrix scrollMatrix = Matrix.Identity;
            if (Scroll != null)
            {
                scroll = Scroll.Value * -1;
                scrollMatrix = Matrix.CreateTranslation((int)Math.Round(scroll.X) + currentTarget.Width / 2, (int)Math.Round(scroll.Y) + currentTarget.Height / 2, 0);
            }
            IsDrawing = true;
            spriteBatch.Begin(sortMode: sortMode, blendState: blendState, samplerState: samplerState, transformMatrix: scrollMatrix, depthStencilState: depthStencilState, effect: currentEffect);
            return this;
        }

        public IRenderService AddRenderTarget(string Key, int Width, int Height, RenderTargetUsage? Usage = RenderTargetUsage.DiscardContents)
        {
            if (GraphicsDevice == null)
            {
                throw new Exception("Attempting to create render target " + Key + "before graphics device initialization");
            }
            targets[Key] = new RenderTarget2D(GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, Usage.Value);
            return this;
        }

        public IRenderService FreeRenderTarget(string Key)
        {
            targets[Key].Dispose();
            targets.Remove(Key);
            return this;
        }

        public IRenderService SetDepth(float Depth)
        {
            depth = Depth;
            return this;
        }

        public IRenderService SetEffect(Effect E)
        {
            currentEffect = E;
            return this;
        }

        public Effect GetEffect()
        {
            return currentEffect;
        }

        public IRenderService SetRenderTarget(string Key)
        {
            return SetRenderTarget(Key, 0, Color.White, 0);
        }

        public IRenderService SetRenderTarget(string Key, Color ClearColor)
        {
            return SetRenderTarget(Key, ClearOptions.Target, ClearColor, 0);
        }

        public IRenderService SetRenderTarget(string Key, float Depth)
        {
            return SetRenderTarget(Key, ClearOptions.DepthBuffer, Color.White, Depth);
        }

        public IRenderService SetRenderTarget(string Key, Color ClearColor, float Depth)
        {
            return SetRenderTarget(Key, ClearOptions.Target | ClearOptions.DepthBuffer, ClearColor, Depth);
        }

        public IRenderService SetRenderTarget(string Key, ClearOptions CO, Color ClearColor, float Depth)
        {
            bool didStop = false;
            if (IsDrawing)
            {
                spriteBatch.End();
                didStop = true;
            }
            currentTarget = Key != null ? targets[Key] : null;
            GraphicsDevice.SetRenderTarget(currentTarget);
            GraphicsDevice.Clear(CO | ClearOptions.Stencil, ClearColor, Depth, 0);
            if (didStop)
            {
                return Start();
            }
            return this;
        }

        public RenderTarget2D GetRenderTarget(string Key)
        {
            return targets[Key];
        }

        public IRenderService StretchCurrentTargetToBackBuffer(bool ShouldLetterbox = false, Color? Tint = null)
        {
            if (currentTarget == null)
            {
                return this;
            }
            RenderTarget2D prevTarget = currentTarget;
            SetRenderTarget(null);
            if (ShouldLetterbox && (ResolutionX % prevTarget.Width != 0 || ResolutionY % prevTarget.Height != 0))
            {
                spriteBatch.Draw(
                    prevTarget,
                    new Rectangle(
                        ResolutionX % prevTarget.Width / 2,
                        ResolutionY % prevTarget.Height / 2,
                        ResolutionX / prevTarget.Width * prevTarget.Width,
                        ResolutionY / prevTarget.Height * prevTarget.Height
                    ),
                    Color.White
                );
            }
            else
            {
                spriteBatch.Draw(
                    prevTarget,
                    new Rectangle(0, 0, ResolutionX, ResolutionY),
                    Tint.HasValue ? Tint.Value : Color.White
                );
            }
            return this;
        }

        public void Cleanup()
        {
            foreach (string key in alphaStacks.Keys)
            {
                alphaStacks[key].ClearStack();
            }
            deferredCalls.Clear();
            worldLockedDeferredCalls.Clear();
        }

        public IRenderService DrawTargetAtPosition(string Key, Vector2 Position, Rectangle? SourceRect = null, Color? Tint = null, float Scale = 1f)
        {
            RenderTarget2D target = targets[Key];
            DrawQuad(
                target,
                Position,
                SourceRect.HasValue ? SourceRect.Value : new Rectangle(0, 0, target.Width, target.Height),
                Tint,
                Scale: Scale
            );
            return this;
        }

        public IRenderService Render(Action A)
        {
            A();
            return this;
        }

        public IRenderService End()
        {
            if (!IsDrawing)
            {
                return this;
            }
            IsDrawing = false;
            spriteBatch.End();
            return this;
        }

        public IRenderService DrawRenderTarget(RenderTarget2D Target, Color? Tint = null)
        {
            return DrawQuad(Target, Vector2.Zero, new Rectangle(0, 0, Target.Width, Target.Height), Tint);
        }

        public IRenderService DrawRenderTarget(string Key, Color? Tint = null)
        {
            return DrawRenderTarget(targets[Key], Tint);
        }

        public IRenderService DrawQuad(Texture2D Texture, Rectangle DestRect, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0f, bool IsTiling = false)
        {
            var contentService = GameService.GetService<IContentService>();
            var metadata = contentService.GetTextureMetadata(Texture);
            if (!IsTiling && metadata != null && contentService.GetAtlasTexture() != null)
            {
                spriteBatch.Draw(
                    contentService.GetAtlasTexture(),
                    DestRect,
                    new Rectangle(SourceRect.X + metadata.X, SourceRect.Y + metadata.Y, SourceRect.Width, SourceRect.Height),
                    Tint ?? Color.White,
                    Rotation,
                    Origin ?? Vector2.Zero,
                    SpriteEffects.None,
                    depth
                );
            }
            else
            {
                spriteBatch.Draw(
                    Texture,
                    DestRect,
                    SourceRect,
                    Tint ?? Color.White,
                    Rotation,
                    Origin ?? Vector2.Zero,
                    SpriteEffects.None,
                    depth
                );
            }
            return this;
        }

        public IRenderService DrawQuad(Texture2D Texture, Vector2 Position, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0f, float Scale = 1f, bool IsTiling = false)
        {
            var contentService = GameService.GetService<IContentService>();
            var metadata = contentService.GetTextureMetadata(Texture);
            if (!IsTiling && metadata != null && contentService.GetAtlasTexture() != null)
            {
                spriteBatch.Draw(
                    contentService.GetAtlasTexture(),
                    Position,
                    new Rectangle(SourceRect.X + metadata.X, SourceRect.Y + metadata.Y, SourceRect.Width, SourceRect.Height),
                    Tint ?? Color.White,
                    Rotation,
                    Origin ?? Vector2.Zero,
                    Scale,
                    SpriteEffects.None,
                    depth
                );
            }
            else
            {
                spriteBatch.Draw(
                    Texture,
                    Position,
                    SourceRect,
                    Tint ?? Color.White,
                    Rotation,
                    Origin ?? Vector2.Zero,
                    Scale,
                    SpriteEffects.None,
                    depth
                );
            }
            return this;
        }

        public IRenderService DrawQuad(string TextureKey, Rectangle DestRect, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0f, bool IsTiling = false)
        {
            return DrawQuad(GameService.GetService<IContentService>().GetxAsset<Texture2D>(TextureKey), DestRect, SourceRect, Tint, Origin, Rotation, IsTiling);
        }

        public IRenderService DrawQuad(string TextureKey, Vector2 Position, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0f, float Scale = 1f, bool IsTiling = false)
        {
            return DrawQuad(GameService.GetService<IContentService>().GetxAsset<Texture2D>(TextureKey), Position, SourceRect, Tint, Origin, Rotation, Scale, IsTiling);
        }

        public IRenderService DrawNinePatch(string TextureKey, Rectangle DestRect, int TileWidth, int TileHeight, Color? Tint = null, Index2? Offset = null)
        {
            Index2 offset = Offset.HasValue ? Offset.Value : Index2.Zero;
            int edgeWidth = Math.Min(DestRect.Width / 2, TileWidth);
            int edgeHeight = Math.Min(DestRect.Height / 2, TileHeight);
            // Draw the corners
            DrawQuad(TextureKey, new Rectangle(DestRect.X, DestRect.Y, edgeWidth, edgeHeight), GraphicsHelper.GetSheetCell(0, 0, TileWidth, TileHeight, offset), Tint);
            DrawQuad(TextureKey, new Rectangle(DestRect.X + DestRect.Width - edgeWidth, DestRect.Y, edgeWidth, edgeHeight), GraphicsHelper.GetSheetCell(2, 0, TileWidth, TileHeight, offset), Tint);
            DrawQuad(TextureKey, new Rectangle(DestRect.X + DestRect.Width - edgeWidth, DestRect.Y + DestRect.Height - edgeHeight, edgeWidth, edgeHeight), GraphicsHelper.GetSheetCell(2, 2, TileWidth, TileHeight, offset), Tint);
            DrawQuad(TextureKey, new Rectangle(DestRect.X, DestRect.Y + DestRect.Height - edgeHeight, edgeWidth, edgeHeight), GraphicsHelper.GetSheetCell(0, 2, TileWidth, TileHeight, offset), Tint);
            bool shouldFill = true;
            // Draw the top and bottom edges
            if (DestRect.Width > TileWidth * 2)
            {
                DrawQuad(TextureKey, new Rectangle(DestRect.X + TileWidth, DestRect.Y, DestRect.Width - TileWidth * 2, edgeHeight), GraphicsHelper.GetSheetCell(1, 0, TileWidth, TileHeight, offset), Tint);
                DrawQuad(TextureKey, new Rectangle(DestRect.X + TileWidth, DestRect.Y + DestRect.Height - edgeHeight, DestRect.Width - TileWidth * 2, edgeHeight), GraphicsHelper.GetSheetCell(1, 2, TileWidth, TileHeight, offset), Tint);
            }
            else
            {
                shouldFill = false;
            }
            // Draw the left and right edges
            if (DestRect.Height > TileHeight * 2)
            {
                DrawQuad(TextureKey, new Rectangle(DestRect.X, DestRect.Y + TileHeight, edgeWidth, DestRect.Height - TileHeight * 2), GraphicsHelper.GetSheetCell(0, 1, TileWidth, TileHeight, offset), Tint);
                DrawQuad(TextureKey, new Rectangle(DestRect.X + DestRect.Width - edgeWidth, DestRect.Y + TileHeight, edgeWidth, DestRect.Height - TileHeight * 2), GraphicsHelper.GetSheetCell(2, 1, TileWidth, TileHeight, offset), Tint);
            }
            else
            {
                shouldFill = false;
            }
            // Fill it
            if (shouldFill)
            {
                DrawQuad(TextureKey, new Rectangle(DestRect.X + TileWidth, DestRect.Y + TileHeight, DestRect.Width - TileWidth * 2, DestRect.Height - TileHeight * 2), GraphicsHelper.GetSheetCell(1, 1, TileWidth, TileHeight, offset), Tint);
            }
            return this;
        }

        public IRenderService DrawString(string SpriteFontKey, string Text, Vector2 Position, Color Tint)
        {
            spriteBatch.DrawString(GameService.GetService<IContentService>().GetxAsset<SpriteFont>(SpriteFontKey), Text, Position, Tint);
            return this;
        }

        public IRenderService DrawDialogLink(string SpriteFontKey, DialogLink Link, Vector2 Position, int MaxDisplayWidth, int MaxLength = -1, float Opacity = 1f)
        {
            if (Link == null || MaxLength == 0)
            {
                return this;
            }
            SpriteFont font = GameService.GetService<IContentService>().GetxAsset<SpriteFont>(SpriteFontKey);
            float offsetX = 0;
            float offsetY = 0;
            int lengthCounter = 0;
            foreach (DialogSegment segment in Link.Segments)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < segment.Text.Length; i++)
                {
                    lengthCounter++;
                    char c = segment.Text[i];
                    sb.Append(c);
                    Vector2 size = font.MeasureString(sb);
                    if (size.X <= (MaxDisplayWidth - offsetX) || c == ' ')
                    {
                        if (i == (segment.Text.Length - 1) || lengthCounter == MaxLength)
                        {
                            DrawString(SpriteFontKey, sb.ToString(), new Vector2(Position.X + offsetX, Position.Y + offsetY), segment.GetRealColor() * Opacity);
                            offsetX += size.X;
                            if (lengthCounter == MaxLength)
                            {
                                return this;
                            }
                        }
                        continue;
                    }
                    else
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                    while (sb[sb.Length - 1] != ' ')
                    {
                        i--;
                        lengthCounter--;
                        sb = sb.Remove(sb.Length - 1, 1);
                    }
                    DrawString(SpriteFontKey, sb.ToString(), new Vector2(Position.X + offsetX, Position.Y + offsetY), segment.GetRealColor() * Opacity);
                    offsetX = 0;
                    offsetY += size.Y;
                    i--;
                    lengthCounter--;
                    sb.Clear();
                }
            }
            return this;
        }

        public IRenderService AddDeferredCall(Action<SpriteBatch> A)
        {
            deferredCalls.Add(A);
            return this;
        }

        public IRenderService FlushDeferredCalls()
        {
            foreach (Action<SpriteBatch> A in deferredCalls)
            {
                A(spriteBatch);
            }
            deferredCalls.Clear();
            return this;
        }

        public IRenderService AddWorldLockedDeferredCall(Action A)
        {
            worldLockedDeferredCalls.Add(A);
            return this;
        }

        public IRenderService FlushWorldLockedDeferredCalls()
        {
            foreach (Action A in worldLockedDeferredCalls)
            {
                A();
            }
            worldLockedDeferredCalls.Clear();
            return this;
        }

        public float GetTargetToDisplayRatioX()
        {
            return (float)GraphicsDevice.PresentationParameters.BackBufferWidth / InternalResolutionX;
        }

        public float GetTargetToDisplayRatioY()
        {
            return (float)GraphicsDevice.PresentationParameters.BackBufferHeight / InternalResolutionY;
        }

        public Texture2D ConvertRenderTargetToTexture(string TargetKey)
        {
            var target = targets[TargetKey];
            var texture = new Texture2D(GraphicsDevice, target.Width, target.Height);
            Color[] targetData = new Color[target.Width * target.Height];
            target.GetData(targetData);
            texture.SetData(targetData);
            return texture;
        }

        public void ExportTargetToPNG(string TargetKey, string Path)
        {
            using (FileStream stream = File.OpenWrite(Path))
            {
                var target = targets[TargetKey];
                target.SaveAsPng(stream, target.Width, target.Height);
            }
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IRenderService);
        }
    }
}
