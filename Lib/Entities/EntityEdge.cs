using WarnerEngine.Lib.Behaviors;

namespace WarnerEngine.Lib.Entities
{
    public class EntityEdge : ISceneEntity
    {
        public IBehavior Behavior { get; private set; }
        public ASceneEntity Parent { get; private set; }
        public ASceneEntity Child { get; private set; }

        public EntityEdge(ASceneEntity Parent, ASceneEntity Child, IBehavior Behavior = null)
        {
            this.Behavior = Behavior;
            this.Parent = Parent;
            this.Child = Child;
        }

        public void OnAdd(Scene ParentScene)
        {
            Child.OnAdd(ParentScene);
        }

        public void OnRemove(Scene ParentScene)
        {
            Child.OnRemove(ParentScene);
        }

        public void PreDraw(float DT)
        {
            Behavior?.Run(Parent, Child);
            Child.PreDraw(DT);
        }

        public void PostDraw()
        {
            Child.PostDraw();
        }
    }
}
