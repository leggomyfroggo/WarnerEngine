using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Graphics
{
    public class GraphicsPipeline
    {
        public class Node
        {
            public Node Next { get; private set; }
            public string Target { get; private set; }
            public string DrawStack { get; private set; }
            public Effect ActiveEffect { get; private set; }
            public DepthStencilState DepthState { get; private set; }
            // TODO: Add ability to flip one target onto the current target

            public Node(string Target, string DrawStack, Effect ActiveEffect = null, DepthStencilState DepthState = null)
            {
                this.Target = Target;
                this.DrawStack = DrawStack;
                this.ActiveEffect = ActiveEffect;
                this.DepthState = DepthState ?? DepthStencilState.Default;
            }

            public Node AttachNext(Node Next)
            {
                this.Next = Next;
                return this;
            }

            public Node PopAndRender() 
            {
                // TODO: Rendering logic here
                return Next;
            }
        }

        public Node StartNode { get; private set; }

        public string OutputTarget
        {
            get
            {
                var currentNode = StartNode;
                while (currentNode.Next != null)
                {
                    currentNode = currentNode.Next;
                }
                return currentNode.Target;
            }
        }

        public GraphicsPipeline(Node StartNode)
        {
            this.StartNode = StartNode;
        }

        public void Render()
        {
            IGraphicsService graphicsService = GameService.GetService<IGraphicsService>();
            Node currentNode = StartNode;
            while (currentNode != null)
            {
                if (!graphicsService.IsDrawing)
                {
                    graphicsService.SetActiveEffect(currentNode.ActiveEffect);
                    graphicsService.SetDepthState(currentNode.DepthState);
                    graphicsService.StartDrawing();
                }
                Node nextNode = currentNode.PopAndRender();
                if (
                    nextNode != null &&
                    (
                        nextNode.ActiveEffect != currentNode.ActiveEffect || 
                        nextNode.DepthState != currentNode.DepthState || 
                        nextNode.Target != currentNode.Target
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
