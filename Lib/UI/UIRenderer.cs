using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.UI
{
    public class UIRenderer : ISceneEntity, IPreDraw, IDraw
    {
        public Func<IUIElement> Builder;

        private string focusedElementKey;

        private bool shouldDrawDeferred;

        private Dictionary<string, Dictionary<string, object>> componentEventStates;
        private Dictionary<string, Dictionary<string, object>> componentStates;

        private UIDrawCall rootDrawCall;
        private bool wasUpdated;

        private string scrollingTargetKey;

        public UIRenderer(Func<IUIElement> Builder, bool ShouldDrawDeferred = false, string ScrollingTargetKey = null)
        {
            this.Builder = Builder;
            shouldDrawDeferred = ShouldDrawDeferred;
            componentEventStates = new Dictionary<string, Dictionary<string, object>>();
            componentStates = new Dictionary<string, Dictionary<string, object>>();
            rootDrawCall = new UIDrawCall();
            wasUpdated = true;
            scrollingTargetKey = ScrollingTargetKey;
        }

        public void OnAdd(Scene ParentScene) { }

        public void OnRemove(Scene ParentScene) { }

        public void PreDraw(float DT)
        {
            if (wasUpdated)
            {
                rootDrawCall = Builder().RenderAsRoot();
                wasUpdated = false;

                // TODO: Clear states for components that were removed
            }

            rootDrawCall.PreDraw(
                DT, 
                false, 
                focusedElementKey, 
                GameService.GetService<IInputService>().GetMouseInScreenSpace()
            );
        }

        public void Draw()
        {
            if (shouldDrawDeferred)
            {
                GameService.GetService<IRenderService>().AddDeferredCall(_ => DrawImplementation());
            }
            else
            {
                DrawImplementation();
            }
        }

        public void DrawImplementation()
        {
            rootDrawCall.Draw(focusedElementKey, this);
        }

        public BackingBox GetBackingBox()
        {
            return BackingBox.Dummy;
        }

        public bool IsVisible()
        {
            return true;
        }

        public void RegisterComponent(IUIElement Element)
        {
            if (!componentStates.ContainsKey(Element.GetKey()))
            {
                componentEventStates[Element.GetKey()] = new Dictionary<string, object>()
                {
                    { "isMouseInside", false },
                    { "scroll", 0 },
                };
                componentStates[Element.GetKey()] = Element.GetDefaultState();
            }
        }

        public TState GetComponentEventState<TState>(string ComponentKey, string StateKey)
        {
            return (TState)componentEventStates[ComponentKey][StateKey];
        }

        public void SetComponentEventState<TState>(string ComponentKey, string StateKey, TState StateValue)
        {
            TState oldState = (TState)componentEventStates[ComponentKey][StateKey];
            if ((oldState == null && StateValue != null) || (oldState != null && !oldState.Equals(StateValue)))
            {
                componentEventStates[ComponentKey][StateKey] = StateValue;
                wasUpdated = true;
            }
        }

        public TState GetComponentState<TState>(string ComponentKey, string StateKey)
        {
            return (TState)componentStates[ComponentKey][StateKey];
        }

        public void SetComponentState<TState>(string ComponentKey, string StateKey, TState StateValue)
        {
            TState oldState = (TState)componentStates[ComponentKey][StateKey];
            if ((oldState == null && StateValue != null) || (oldState != null && !oldState.Equals(StateValue)))
            {
                componentStates[ComponentKey][StateKey] = StateValue;
                wasUpdated = true;
            }
        }

        public void ForceUpdate()
        {
            wasUpdated = true;
        }

        public void SetFocusedElement(string Key)
        {
            focusedElementKey = Key;
        }

        public void EnableScrollingTarget(UIDrawCall DrawCall)
        {
            GameService.GetService<IRenderService>()
                .End()
                .SetRenderTarget(scrollingTargetKey, Color.Transparent)
                .Start(new Vector2(DrawCall.X, DrawCall.Y), Enums.ScrollReference.TopLeft);
        }

        public void FlipScrollingTarget(UIDrawCall DrawCall)
        {
            GameService.GetService<IRenderService>()
                .End()
                .SetRenderTarget(Services.Implementations.SceneService.RenderTargets.CompositeTertiary)
                .Start()
                .DrawTargetAtPosition(
                    scrollingTargetKey, 
                    new Vector2(DrawCall.X, DrawCall.Y),
                    SourceRect: new Rectangle(0, 0, DrawCall.Width, DrawCall.Height)
                );
        }
    }
}
