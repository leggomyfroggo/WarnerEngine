using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public class SceneService : Service
    {
        public const string DEPTH_TARGET_KEY = "scene_depth";
        public const string BUFFER_TARGET_KEY = "scene_buffer";
        public const string COMPOSITE_TARGET_KEY = "scene_composite";

        private static readonly ServiceCompositionMetadata COMPOSITION_METADATA = new ServiceCompositionMetadata()
        {
            RenderTargetKey = COMPOSITE_TARGET_KEY,
            Position = Vector2.Zero,
            Priority = 0,
            Tint = Color.White,
        };

        private Dictionary<string, Scene> scenes;
        private bool shouldReset;
        public Scene CurrentScene { get; private set; }

        public override HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() { typeof(EventService) };
        }

        public override void Initialize()
        {
            GameService.GetService<EventService>().Subscribe(
                Events.INTERNAL_RESOLUTION_CHANGED,
                _ =>
                {
                    RenderService renderService = GameService.GetService<RenderService>();
                    renderService
                        .AddRenderTarget(
                            DEPTH_TARGET_KEY,
                            renderService.InternalResolutionX,
                            renderService.InternalResolutionY,
                            RenderTargetUsage.PreserveContents
                        )
                        .AddRenderTarget(
                            BUFFER_TARGET_KEY,
                            renderService.InternalResolutionX,
                            renderService.InternalResolutionY,
                            RenderTargetUsage.PreserveContents
                        )
                        .AddRenderTarget(
                            COMPOSITE_TARGET_KEY,
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

        public SceneService RegisterScene(string Key, Scene L)
        {
            scenes[Key] = L;
            return this;
        }

        public SceneService SetScene(string Key)
        {
            if (CurrentScene != null)
            {
                CurrentScene.OnSceneEnd();
            }
            CurrentScene = scenes[Key];
            CurrentScene.OnSceneStart();

            return this;
        }

        public SceneService ResetCurrentScene()
        {
            shouldReset = true;
            return this;
        }

        public override void PreDraw(float DT)
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

        public override ServiceCompositionMetadata Draw()
        {
            if (CurrentScene != null)
            {
                CurrentScene.Draw();
                return new ServiceCompositionMetadata()
                {
                    RenderTargetKey = COMPOSITE_TARGET_KEY,
                    Position = Vector2.Zero,
                    Priority = 0,
                    Tint = Color.White,
                    CompositeEffect = CurrentScene.GetCompositeEffect(),
                };
            }
            return ServiceCompositionMetadata.Empty;
        }

        public override void PostDraw()
        {
            if (CurrentScene != null)
            {
                CurrentScene.PostDraw();
            }
        }

        public override Type GetBackingInterfaceType()
        {
            return typeof(SceneService);
        }
    }
}
