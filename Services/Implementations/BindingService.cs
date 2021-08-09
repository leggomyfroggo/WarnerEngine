using System;
using System.Collections.Generic;

using WarnerEngine.Lib.Bindings;
using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Services.Implementations
{
    public class BindingService : IBindingService
    {
        private LinkedList<IBinding> _bindings;
        private Dictionary<ISceneEntity, HashSet<LinkedListNode<IBinding>>> _perEntityBindings;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize()
        {
            _bindings = new LinkedList<IBinding>();
            _perEntityBindings = new Dictionary<ISceneEntity, HashSet<LinkedListNode<IBinding>>>();
        }

        public void AddBinding(IBinding Binding)
        {
            var bindingNode = _bindings.AddLast(Binding);
            RegisterEntityBinding(Binding.GetLeftEntity(), bindingNode);
            RegisterEntityBinding(Binding.GetRightEntity(), bindingNode);
        }

        public void RemoveBindingsForEntity(ISceneEntity Entity)
        {
            if (!_perEntityBindings.ContainsKey(Entity))
            {
                return;
            }
            foreach (var bindingNode in _perEntityBindings[Entity])
            {
                var binding = bindingNode.Value;
                ISceneEntity otherEntity;
                if (binding.GetLeftEntity() == Entity)
                {
                    otherEntity = binding.GetRightEntity();
                    binding.OnLeftRemoved();
                }
                else
                {
                    otherEntity = binding.GetLeftEntity();
                }
                _perEntityBindings[otherEntity].Remove(bindingNode);
                _perEntityBindings[Entity].Remove(bindingNode);
                _bindings.Remove(bindingNode);
            }
            _perEntityBindings.Remove(Entity);
        }

        public void PreDraw(float DT) 
        {
            var head = _bindings.First;
            while (head != null)
            {
                var shouldDelete = head.Value.ProcessBinding();
                var toDelete = head;
                head = toDelete.Next;
                if (shouldDelete)
                {
                    _bindings.Remove(toDelete);
                }
            }
        }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public Type GetBackingInterfaceType()
        {
            return typeof(IBindingService);
        }

        private void RegisterEntityBinding(ISceneEntity Entity, LinkedListNode<IBinding> Binding)
        {
            if (!_perEntityBindings.ContainsKey(Entity))
            {
                _perEntityBindings[Entity] = new HashSet<LinkedListNode<IBinding>>();
            }
            _perEntityBindings[Entity].Add(Binding);
        }
    }
}
