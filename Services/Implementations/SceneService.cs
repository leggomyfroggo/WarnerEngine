using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;

namespace WarnerEngine.Services.Implementations
{
    public class SceneService : ISceneService
    {
        public static class RenderTargets {
            public const string DepthPrimary = "scene_depth";
            public const string CompositeSecondary = "scene_composite_secondary";
            public const string CompositeTertiary = "scene_composite_tertiary";
        }

        private Dictionary<string, Scene> scenes;
        private bool shouldReset;
        public Scene CurrentScene { get; private set; }

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

            scenes = new Dictionary<string, Scene>();
            shouldReset = false;
            CurrentScene = null;
        }

        public ISceneService RegisterScene(string Key, Scene L)
        {
            scenes[Key] = L;
            return this;
        }

        public ISceneService SetScene(string Key)
        {
            if (CurrentScene != null)
            {
                CurrentScene.OnSceneEnd();
            }
            CurrentScene = scenes[Key];
            CurrentScene.OnSceneStart();

            return this;
        }

        public ISceneService ResetCurrentScene()
        {
            shouldReset = true;
            return this;
        }

        public void PreDraw(float DT)
        {
            if (CurrentScene != null)
            {
                if (shouldReset)
                {
                    CurrentScene.OnSceneEnd();
                    CurrentScene.OnSceneStart();
                    shouldReset = false;
                }
                CurrentScene.PreDraw(DT);
            }
        }

        public ServiceCompositionMetadata Draw()
        {
            if (CurrentScene != null)
            {
                CurrentScene.Draw();
                return new ServiceCompositionMetadata()
                {
                    RenderTargetKey = RenderTargets.CompositeTertiary,
                    Position = Vector2.Zero,
                    Priority = 0,
                    Tint = Color.White,
                    CompositeEffect = CurrentScene.GetCompositeEffect(),
                };
            }
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw()
        {
            if (CurrentScene != null)
            {
                CurrentScene.PostDraw();
            }
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(ISceneService);
        }
    }
}
