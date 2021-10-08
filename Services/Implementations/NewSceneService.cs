using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.SceneGraph;

namespace WarnerEngine.Services.Implementations
{
    public class NewSceneService : INewSceneService
    {
        public static class RenderTargets
        {
            public const string DepthPrimary = "scene_depth";
            public const string CompositeSecondary = "scene_composite_secondary";
            public const string CompositeTertiary = "scene_composite_tertiary";
        }

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
            GameService.GetService<IEventService>().Subscribe(
                Events.INTERNAL_RESOLUTION_CHANGED,
                _ =>
                {
                    IRenderService renderService = GameService.GetService<IRenderService>();
                    renderService
                        .AddRenderTarget(
                            RenderTargets.DepthPrimary,
                            renderService.InternalResolutionX,
                            renderService.InternalResolutionY,
                            RenderTargetUsage.PreserveContents
                        )
                        .AddRenderTarget(
                            RenderTargets.CompositeSecondary,
                            renderService.InternalResolutionX,
                            renderService.InternalResolutionY,
                            RenderTargetUsage.PreserveContents
                        )
                        .AddRenderTarget(
                            RenderTargets.CompositeTertiary,
                            renderService.InternalResolutionX,
                            renderService.InternalResolutionY,
                            RenderTargetUsage.PreserveContents
                        );
                }
            );

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
