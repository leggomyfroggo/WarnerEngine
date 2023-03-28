using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Graphics
{
    public class GraphicsPipeline
    {
        public IGraphicsPipelineNode StartNode { get; private set; }

        public GraphicsPipeline(IGraphicsPipelineNode StartNode, params IGraphicsPipelineNode[] Nodes)
        {
            this.StartNode = StartNode;
            var currentNode = this.StartNode;
            foreach (IGraphicsPipelineNode node in Nodes)
            {
                currentNode = currentNode.Extend(node);
            }
        }

        public void Render()
        {
            IGraphicsService graphicsService = GameService.GetService<IGraphicsService>();
            IGraphicsPipelineNode currentNode = StartNode;
            while (currentNode != null)
            {
                if (!graphicsService.IsDrawing)
                {
                    currentNode.Prep();
                    graphicsService.StartDrawing();
                }
                IGraphicsPipelineNode nextNode = currentNode.PopAndRender();
                if (
                    nextNode != null &&
                    (
                        nextNode.ActiveEffect != currentNode.ActiveEffect || 
                        nextNode.DepthState != currentNode.DepthState || 
                        nextNode.TargetID != currentNode.TargetID
                    )
                )
                {
                    graphicsService.EndDrawing();
                }
                currentNode = nextNode;
            }
            graphicsService.EndDrawing();
        }
    }
}
