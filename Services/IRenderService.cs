using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Dialog;

namespace WarnerEngine.Services
{
    public interface IRenderService : IService
    {
        GraphicsDevice GraphicsDevice { get; }
        int InternalResolutionX { get; }
        int InternalResolutionY { get; }
        bool IsDrawing { get; }
        int ResolutionX { get; }
        int ResolutionY { get; }

        IRenderService AddDeferredCall(Action<SpriteBatch> A);
        IRenderService AddRenderTarget(string Key, int Width, int Height, RenderTargetUsage? Usage = RenderTargetUsage.DiscardContents);
        IRenderService AddWorldLockedDeferredCall(Action A);
        IRenderService BuildAlphaStack(string Key);
        void Cleanup();
        IRenderService ConditionallyRunPipelineBlock(Func<IRenderService, bool> ConditionFunction, Func<IRenderService, IRenderService> PipelineBlock);
        Texture2D ConvertRenderTargetToTexture(string TargetKey);
        IRenderService DrawAlphaStack(string Key, Color? Tint = null);
        IRenderService DrawDialogLink(string SpriteFontKey, DialogLink Link, Vector2 Position, int MaxDisplayWidth, int MaxLength = -1, float Opacity = 1);
        IRenderService DrawNinePatch(string TextureKey, Rectangle DestRect, int TileWidth, int TileHeight, Color? Tint = null, Index2? Offset = null);
        IRenderService DrawQuad(string TextureKey, Rectangle DestRect, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0, bool IsTiling = false);
        IRenderService DrawQuad(string TextureKey, Vector2 Position, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0, float Scale = 1, bool IsTiling = false);
        IRenderService DrawQuad(Texture2D Texture, Rectangle DestRect, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0, bool IsTiling = false);
        IRenderService DrawQuad(Texture2D Texture, Vector2 Position, Rectangle SourceRect, Color? Tint = null, Vector2? Origin = null, float Rotation = 0, float Scale = 1, bool IsTiling = false);
        IRenderService DrawRenderTarget(RenderTarget2D Target, Color? Tint = null);
        IRenderService DrawRenderTarget(string Key, Color? Tint = null);
        IRenderService DrawString(string SpriteFontKey, string Text, Vector2 Position, Color Tint);
        IRenderService DrawTargetAtPosition(string Key, Vector2 Position, Rectangle? SourceRect = null, Color? Tint = null, float Scale = 1);
        IRenderService End();
        void ExportTargetToPNG(string TargetKey, string Path);
        IRenderService FlushAlphaStack(string Key, Color? Tint = null);
        IRenderService FlushDeferredCalls();
        IRenderService FlushWorldLockedDeferredCalls();
        IRenderService FreeRenderTarget(string Key);
        int GetAlphaStackFragmentCount(string Key);
        Effect GetEffect();
        RenderTarget2D GetRenderTarget(string Key);
        float GetTargetToDisplayRatioX();
        float GetTargetToDisplayRatioY();
        IRenderService PushAlphaFragment(string Key, Texture2D Texture, Rectangle DestinationRectangle, Rectangle SourceRectangle, float Opacity = 1, float Rotation = 0, Vector2? Origin = null, Color? Tint = null, bool IsTiling = false);
        IRenderService Render(Action A);
        IRenderService SetBlendState(BlendState BlendState);
        IRenderService SetDepth(float Depth);
        IRenderService SetDepthStencilState(DepthStencilState DSS);
        IRenderService SetEffect(Effect E);
        IRenderService SetGraphicsDevice(GraphicsDevice GD);
        IRenderService SetInternalResolution(int X, int Y);
        IRenderService SetRenderTarget(string Key);
        IRenderService SetRenderTarget(string Key, ClearOptions CO, Color ClearColor, float Depth);
        IRenderService SetRenderTarget(string Key, Color ClearColor);
        IRenderService SetRenderTarget(string Key, Color ClearColor, float Depth);
        IRenderService SetRenderTarget(string Key, float Depth);
        IRenderService SetSamplerState(SamplerState SS);
        IRenderService SetSortMode(SpriteSortMode SortMode);
        IRenderService Start(Vector2? Scroll = null, Enums.ScrollReference ScrollReference = Enums.ScrollReference.Center);
        IRenderService StretchCurrentTargetToBackBuffer(bool ShouldLetterbox = false, Color? Tint = null);
    }
}
