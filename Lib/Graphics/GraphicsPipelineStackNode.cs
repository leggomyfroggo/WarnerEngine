using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Graphics
{
    public class GraphicsPipelineStackNode : AGraphicsPipelineNode
    {
        private string _drawStackID;

        public GraphicsPipelineStackNode(string DrawStackID, string TargetID, Effect ActiveEffect = null, DepthStencilState DepthState = null, Color? ClearColor = null) : base(TargetID, ActiveEffect, DepthState, ClearColor)
        {
            _drawStackID = DrawStackID;
        }

        protected override void Render()
        {
            GameService.GetService<IGraphicsService>().FlushDrawStack(_drawStackID);
        }
    }
}
