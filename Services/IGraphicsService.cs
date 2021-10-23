using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Graphics;

namespace WarnerEngine.Services
{
    public interface IGraphicsService : IService
    {
        GraphicsDevice GD { get; }
        int InternalResolutionX { get; }
        int InternalResolutionY { get; }
        int ResolutionX { get; }
        int ResolutionY { get; }
        bool IsDrawing { get; }
        GraphicsPipeline Pipeline { get; }

        IGraphicsService SetGraphicsDevice(GraphicsDevice GD);
        IGraphicsService AddDrawStack(string ID, int Size);
        IGraphicsService FlushDrawStack(string ID);
        IGraphicsService DrawDrawStack(string ID);
        IGraphicsService SetPipeline(GraphicsPipeline P);
        IGraphicsService ExecutePipeline();
        IGraphicsService AddTarget(string ID, int Width, int Height);
        IGraphicsService SetTarget(string ID);
        RenderTarget2D GetTarget(string ID);
        IGraphicsService SetActiveEffect(Effect E);
        IGraphicsService SetDepthState(DepthStencilState DepthState);
        IGraphicsService SetActiveSamplerState(SamplerState ActiveSamplerState);
        IGraphicsService StartDrawing();
        IGraphicsService EndDrawing();
        IGraphicsService PushDrawCall(string DrawStackID, DrawCall DC);
        IGraphicsService DrawSprite(string TextureID, Rectangle Source, Rectangle Target, Color Tint);
        IGraphicsService DrawSprite(Texture2D Texture, Rectangle Source, Rectangle Target, Color Tint);
    }
}
