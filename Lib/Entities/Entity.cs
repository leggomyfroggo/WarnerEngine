using System.Collections.Generic;
using System.Linq;

using ProjectWarnerShared.Lib.Components;
using ProjectWarnerShared.Lib.Entities;
using ProjectWarnerShared.Scenes;

namespace ProjectWarnerShared.Lib.Entities
{
    public abstract class Entity : ISceneEntity, IPreDraw, IPostDraw
    {
        protected List<IComponent> components;

        public Entity()
        {
            components = new List<IComponent>();
        }

        public virtual void OnAdd(Scene ParentScene) { }

        public virtual void OnRemove(Scene ParentScene) { }

        public void PostDraw()
        {
            foreach (IComponent component in components)
            {
                component.PostDraw();
            }
            PostDrawImplementation();
        }

        public virtual void PostDrawImplementation() { }

        public void PreDraw(float DT)
        {
            foreach (IComponent component in components)
            {
                component.PreDraw(DT);
            }
            PreDrawImplementation(DT);
        }

        public virtual void PreDrawImplementation(float DT) { }

        public T GetComponent<T>() where T : IComponent
        {
            return components.OfType<T>().First();
        }

        public void RegisterComponent(IComponent Component)
        {
            components.Add(Component);
        }
    }
}
