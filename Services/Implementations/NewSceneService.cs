using System;
using System.Collections.Generic;

using WarnerEngine.Lib.SceneGraph;

namespace WarnerEngine.Services.Implementations
{
    public class NewSceneService : INewSceneService
    {
        public IScene CurrentScene { get; private set; }

        private Dictionary<string, IScene> _scenes;

        public Type GetBackingInterfaceType()
        {
            return typeof(INewSceneService);
        }

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() { typeof(IEventService) };
        }

        public void Initialize() 
        {
            _scenes = new Dictionary<string, IScene>();
        }

        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public INewSceneService RegisterScene(IScene Scene)
        {
            _scenes[Scene.Name] = Scene;
            return this;
        }

        public INewSceneService SetScene(string SceneName)
        {
            CurrentScene = _scenes[SceneName];
            return this;
        }
    }
}
