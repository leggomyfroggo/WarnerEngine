using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Graphics;

namespace WarnerEngine.Services.Implementations
{
    public class GraphicsService : IGraphicsService
    {
        private SpriteBatch _spriteBatch;

        private Dictionary<string, DrawStack> _drawStacks;

        private Dictionary<string, RenderTarget2D> _targets;

        private Effect _activeEffect;
        private DepthStencilState _depthState;
        private SamplerState _activeSamplerState;

        public GraphicsDevice GD { get; private set; }

        public int InternalResolutionX { get; private set; }

        public int InternalResolutionY { get; private set; }

        public int ResolutionX { get; private set; }

        public int ResolutionY { get; private set; }

        public bool IsDrawing { get; private set; }

        public GraphicsPipeline Pipeline { get; private set; }

        public Type GetBackingInterfaceType()
        {
            return typeof(IGraphicsService);
        }

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize() 
        {
            _drawStacks = new Dictionary<string, DrawStack>();
            _targets = new Dictionary<string, RenderTarget2D>();
        }

        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public IGraphicsService SetGraphicsDevice(GraphicsDevice GD)
        {
            this.GD = GD;
            _spriteBatch = new SpriteBatch(GD);
            return this;
        }

        public IGraphicsService AddDrawStack(string ID, int Size = 100)
        {
            _drawStacks[ID] = new DrawStack(ID, Size);
            return this;
        }

        public IGraphicsService FlushDrawStack(string ID)
        {
            _drawStacks[ID].Flush();
            return this;
        }

        public IGraphicsService DrawDrawStack(string ID)
        {
            _drawStacks[ID].Draw();
            return this;
        }

        public IGraphicsService SetPipeline(GraphicsPipeline P)
        {
            Pipeline = P;
            return this;
        }

        public IGraphicsService ExecutePipeline()
        {
            Pipeline.Render();
            return this;
        }

        public IGraphicsService AddTarget(string ID, int Width, int Height)
        {
            _targets[ID] = new RenderTarget2D(GD, Width, Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            return this;
        }

        public IGraphicsService SetTarget(string ID)
        {
            GD.SetRenderTarget(ID == null ? null : _targets[ID]);
            return this;
        }

        public RenderTarget2D GetTarget(string ID)
        {
            return _targets[ID];
        }

        public IGraphicsService SetActiveEffect(Effect E)
        {
            _activeEffect = E;
            return this;
        }

        public IGraphicsService SetDepthState(DepthStencilState DepthState)
        {
            _depthState = DepthState;
            return this;
        }

        public IGraphicsService SetActiveSamplerState(SamplerState ActiveSamplerState)
        {
            _activeSamplerState = ActiveSamplerState;
            return this;
        }

        public IGraphicsService StartDrawing()
        {
            if (IsDrawing)
            {
                throw new Exception("Drawing has already started");
            }
            // TODO: Pass in effect, transform, depth state, etc
            IsDrawing = true;
            _spriteBatch.Begin(samplerState: _activeSamplerState, effect: _activeEffect, depthStencilState: _depthState);
            return this;
        }

        public IGraphicsService EndDrawing()
        {
            if (!IsDrawing)
            {
                throw new Exception("Drawing has already ended");
            }
            IsDrawing = false;
            _spriteBatch.End();
            return this;
        }

        public IGraphicsService PushDrawCall(string DrawStackID, DrawCall DC)
        {
            _drawStacks[DrawStackID].PushDrawCall(DC);
            return this;
        }

        public IGraphicsService DrawSprite(string TextureID, Rectangle Target, Rectangle Source, Color Tint)
        {
            return DrawSprite(
                GameService.GetService<IContentService>().GetAsset<Texture2D>(TextureID),
                Target,
                Source,
                Tint
            );
        }

        public IGraphicsService DrawSprite(Texture2D Texture, Rectangle Target, Rectangle Source, Color Tint)
        {
            _spriteBatch.Draw(
                Texture,
                Target,
                Source,
                Tint
            );
            return this;
        }
    }
}
