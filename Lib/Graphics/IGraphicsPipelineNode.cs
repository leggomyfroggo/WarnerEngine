using Microsoft.Xna.Framework.Graphics;

namespace WarnerEngine.Lib.Graphics
{
    public interface IGraphicsPipelineNode
    {
        IGraphicsPipelineNode Next { get; }
        string TargetID { get; }
        Effect ActiveEffect { get; }
        DepthStencilState DepthState { get; }

        IGraphicsPipelineNode Extend(IGraphicsPipelineNode Next);
        IGraphicsPipelineNode PopAndRender();
        IGraphicsPipelineNode Prep();
    }
}
