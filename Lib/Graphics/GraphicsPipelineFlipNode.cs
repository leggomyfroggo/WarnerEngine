using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Graphics
{
    public class GraphicsPipelineFlipNode : AGraphicsPipelineNode
    {
        private string _sourceTargetID;

        public GraphicsPipelineFlipNode(string SourceTargetID, string TargetID, Effect ActiveEffect = null, DepthStencilState DepthState = null, Color? ClearColor = null) : base(TargetID, ActiveEffect, DepthState, ClearColor)
        {
            _sourceTargetID = SourceTargetID;
        }

        protected override void Render()
        {
            var graphicsService = GameService.GetService<IGraphicsService>();
            var sourceTarget = graphicsService.GetTarget(_sourceTargetID);
            if (TargetID == null)
            {
                graphicsService.DrawSprite(
                    graphicsService.GetTarget(_sourceTargetID),
                    new Rectangle(0, 0, 800, 480),
                    new Rectangle(0, 0, sourceTarget.Width, sourceTarget.Height),
                    Color.White
                );
            }
            else
            {
                var target = graphicsService.GetTarget(TargetID);
                graphicsService.DrawSprite(
                    graphicsService.GetTarget(_sourceTargetID),
                    new Rectangle(0, 0, target.Width, target.Height),
                    new Rectangle(0, 0, sourceTarget.Width, sourceTarget.Height),
                    Color.White
                );
            }
        }
    }
}
