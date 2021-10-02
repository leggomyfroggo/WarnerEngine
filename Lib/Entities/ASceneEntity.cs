using System.Collections.Generic;

using WarnerEngine.Lib.Behaviors;

namespace WarnerEngine.Lib.Entities
{
    public abstract class ASceneEntity : ISceneEntity, IPreDraw, IPostDraw
    {
        private List<EntityEdge> _childEdges;

        public ASceneEntity()
        {
            _childEdges = new List<EntityEdge>();
        }

        public void OnAdd(Scene ParentScene)
        {
            OnAddedToScene(ParentScene);
            foreach (EntityEdge edge in _childEdges)
            {
                edge.OnAdd(ParentScene);
            }
        }

        protected abstract void OnAddedToScene(Scene ParentScene);

        public void OnRemove(Scene ParentScene)
        {
            OnRemovedFromScene(ParentScene);
            foreach (EntityEdge edge in _childEdges)
            { 
                edge.OnRemove(ParentScene);
            }
        }

        protected abstract void OnRemovedFromScene(Scene ParentScene);

        public void PreDraw(float DT)
        {
            OnPreDraw(DT);
            foreach (EntityEdge edge in _childEdges)
            {
                edge.PreDraw(DT);
            }
        }

        protected abstract void OnPreDraw(float DT);

        public void PostDraw()
        {
            OnPostDraw();
            foreach (EntityEdge edge in _childEdges)
            {
                edge.PostDraw();
            }
        }

        protected abstract void OnPostDraw();

        protected void AddChild<TChild, TBehavior>(TChild Child, ABehavior<TBehavior> Behavior = null) where TChild : ASceneEntity, TBehavior
        {
            _childEdges.Add(new EntityEdge(this, Child, Behavior));
        }
    }
}
