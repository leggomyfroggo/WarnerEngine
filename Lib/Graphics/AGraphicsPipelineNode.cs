using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Graphics
{
    public abstract class AGraphicsPipelineNode : IGraphicsPipelineNode
    {
        public IGraphicsPipelineNode Next { get; protected set; }

        public string TargetID { get; protected set; }

        public Effect ActiveEffect { get; protected set; }

        public DepthStencilState DepthState { get; protected set; }

        public Color? ClearColor { get; protected set; }

        public AGraphicsPipelineNode(string TargetID, Effect ActiveEffect, DepthStencilState DepthState, Color? ClearColor)
        {
            this.TargetID = TargetID;
            this.ActiveEffect = ActiveEffect;
            this.DepthState = DepthState ?? DepthStencilState.Default;
            this.ClearColor = ClearColor;
        }

        public IGraphicsPipelineNode Extend(IGraphicsPipelineNode Next)
        {
            this.Next = Next;
            return this.Next;
        }

        public IGraphicsPipelineNode PopAndRender()
        {
            Render();
            return Next;
        }

        protected abstract void Render();

        public IGraphicsPipelineNode Prep()
        {
            IGraphicsService graphicsService = GameService.GetService<IGraphicsService>();
            graphicsService
                .SetTarget(TargetID)
                .SetActiveEffect(ActiveEffect)
                .SetDepthState(DepthState);
            if (ClearColor != null)
            {
                graphicsService.GD.Clear(ClearColor.Value);
            }
            return this;
        }
    }
}
