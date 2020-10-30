using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#if ANDROID
using Microsoft.Xna.Framework.Input.Touch;
#endif

using WarnerEngine.Lib;
using WarnerEngine.Lib.Components.Physics;
using WarnerEngine.Lib.Helpers;

namespace WarnerEngine.Services.Implementations
{
    public class InputService : IInputService
    {
        public const int REVOLVING_BUFFER_SIZE = 15;
        public const int TAP_FRAME_THRESHOLD = 4;

        private const float STICK_THRESHOLD = 0.3f;

        private Dictionary<InputAction, Keys> actionToKey;
        private Dictionary<InputAction, Buttons> actionToButton;

        private KeyboardState[] keyState;
        private GamePadState[] padState;
        private MouseState[] mouseState;
        private int revolvingIndex;

        private HashSet<InputAction> consumedActions;

        private AutoTween leftRumble;
        private AutoTween rightRumble;

        private bool isClickPropagationPaused;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize()
        {
            actionToKey = new Dictionary<InputAction, Keys>();
            actionToButton = new Dictionary<InputAction, Buttons>();

            keyState = new KeyboardState[REVOLVING_BUFFER_SIZE];
            padState = new GamePadState[REVOLVING_BUFFER_SIZE];
            mouseState = new MouseState[REVOLVING_BUFFER_SIZE];
            revolvingIndex = -1;

            consumedActions = new HashSet<InputAction>();
        }

        public void PreDraw(float DT)
        {
            revolvingIndex = (revolvingIndex + 1) % REVOLVING_BUFFER_SIZE;

            keyState[revolvingIndex] = Keyboard.GetState();
            padState[revolvingIndex] = GamePad.GetState(0);
            mouseState[revolvingIndex] = Mouse.GetState();
            isClickPropagationPaused = false;

            consumedActions.Clear();

            float leftRumbleStrength = 0;
            float rightRumbleStrength = 0;
            if (leftRumble != null)
            {
                if (leftRumble.IsRunning)
                {
                    leftRumbleStrength = leftRumble.GetTween();
                }
            }
            if (rightRumble != null)
            {
                if (rightRumble.IsRunning)
                {
                    rightRumbleStrength = rightRumble.GetTween();
                }
            }
            GamePad.SetVibration(0, leftRumbleStrength, rightRumbleStrength);
        }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public IInputService RegisterInput(InputAction Action, Keys? Key = null, Buttons? Button = null)
        {
            if (Key.HasValue)
            {
                actionToKey[Action] = (Keys)Key;
            }
            if (Button.HasValue)
            {
                actionToButton[Action] = (Buttons)Button;
            }
            return this;
        }

        public bool IsActionHeld(InputAction Action)
        {
            bool isKeyDown = false;
            bool isButtonDown = false;
            if (actionToKey.ContainsKey(Action))
            {
                isKeyDown = GetCurrentKeyState().IsKeyDown(actionToKey[Action]);
            }
            if (actionToButton.ContainsKey(Action))
            {
                isButtonDown = GetCurrentPadState().IsButtonDown(actionToButton[Action]);
            }
            return isKeyDown || isButtonDown;
        }

        public bool WasActionPressed(InputAction Action, bool Consume = false)
        {
            if (consumedActions.Contains(Action))
            {
                return false;
            }
            bool wasKeyPressed = false;
            bool wasButtonPressed = false;
            if (actionToKey.ContainsKey(Action))
            {
                Keys key = actionToKey[Action];
                wasKeyPressed = GetCurrentKeyState().IsKeyDown(key) && GetLastFrameKeyState().IsKeyUp(key);
            }
            if (actionToButton.ContainsKey(Action))
            {
                Buttons button = actionToButton[Action];
                wasButtonPressed = GetCurrentPadState().IsButtonDown(button) && GetLastFramePadState().IsButtonUp(button);
            }
            bool wasActionPressed = wasKeyPressed || wasButtonPressed;
            if (Consume && wasActionPressed)
            {
                consumedActions.Add(Action);
            }
            return wasActionPressed;
        }

        public bool WasActionDoubleTapped(InputAction Action)
        {
            bool isInKeyFindMode = false;
            int keyIslandCount = 0;
            bool isInButtonFindMode = false;
            int buttonIslandCount = 0;
            for (int i = 0; i < REVOLVING_BUFFER_SIZE; i++)
            {
                if (GetRolledbackKeyState(i).IsKeyUp(actionToKey[Action]))
                {
                    isInKeyFindMode = true;
                }
                else if (GetRolledbackKeyState(i).IsKeyDown(actionToKey[Action]) && isInKeyFindMode)
                {
                    keyIslandCount++;
                    isInKeyFindMode = false;
                }
                if (GetRolledbackPadState(i).IsButtonUp(actionToButton[Action]))
                {
                    isInButtonFindMode = true;
                }
                else if (GetRolledbackPadState(i).IsButtonDown(actionToButton[Action]) && isInButtonFindMode)
                {
                    buttonIslandCount++;
                    isInButtonFindMode = false;
                }
            }
            return keyIslandCount >= 2 || buttonIslandCount >= 2;
        }

        public Enums.Direction? GetDirectionDoubleTapped()
        {
            if (WasActionDoubleTapped(InputAction.Left))
            {
                return Enums.Direction.left;
            }
            else if (WasActionDoubleTapped(InputAction.Up))
            {
                return Enums.Direction.up;
            }
            else if (WasActionDoubleTapped(InputAction.Right))
            {
                return Enums.Direction.right;
            }
            else if (WasActionDoubleTapped(InputAction.Down))
            {
                return Enums.Direction.down;
            }
            return null;
        }

        public bool WasActionTapped(InputAction Action)
        {
            bool isInInputFindMode = false;
            int heldFrames = 0;
            for (int i = 0; i < REVOLVING_BUFFER_SIZE; i++)
            {
                if (GetRolledbackKeyState(i).IsKeyUp(actionToKey[Action]) && GetRolledbackPadState(i).IsButtonUp(actionToButton[Action]))
                {
                    if (isInInputFindMode)
                    {
                        break;
                    }
                    isInInputFindMode = true;
                }
                else if (isInInputFindMode && (GetRolledbackKeyState(i).IsKeyDown(actionToKey[Action]) || GetRolledbackPadState(i).IsButtonDown(actionToButton[Action])))
                {
                    heldFrames++;
                }
            }
            return heldFrames > 0 && heldFrames <= TAP_FRAME_THRESHOLD;
        }

#if ANDROID
        public Vector2 GetLeftStickVector(int Rollback = 0)
        {
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count == 0)
            {
                return Vector2.Zero;
            }
            TouchLocation touch = touches[0];
            Vector2 touchVec = touch.Position - new Vector2(1920 / 2, 1080 / 2);
            touchVec.Normalize();
            return touchVec;
        }
#else
        public Vector2 GetLeftStickVector(int Rollback = 0)
        {
            Vector2 vec = GetRolledbackPadState(Rollback).ThumbSticks.Left;
            vec.Y *= -1;
            return vec.Length() > STICK_THRESHOLD ? vec : Vector2.Zero;
        }
#endif

        public Enums.Direction? GetLeftStickTapDirection()
        {
            Vector2 leftStickVector = GetLeftStickVector();
            if (leftStickVector != Vector2.Zero)
            {
                return null;
            }
            Enums.Direction? maybeTappedDirection = null;
            for (int i = 1; i < REVOLVING_BUFFER_SIZE; i++)
            {
                Vector2 rolledbackLeftStickVector = GetLeftStickVector(i);
                if (maybeTappedDirection != null)
                {
                    if (rolledbackLeftStickVector == Vector2.Zero)
                    {
                        return maybeTappedDirection;
                    }
                    Enums.Direction currentTappedDirection = GraphicsHelper.GetDirectionFromVector(rolledbackLeftStickVector);
                    if (currentTappedDirection != maybeTappedDirection)
                    {
                        return null;
                    }
                }
                else
                {
                    if (rolledbackLeftStickVector != Vector2.Zero)
                    {
                        maybeTappedDirection = GraphicsHelper.GetDirectionFromVector(rolledbackLeftStickVector);
                    }
                }
            }
            return null;
        }

        public bool IsLeftShiftHeld() => GetCurrentKeyState().IsKeyDown(Keys.LeftShift);
        public bool IsLeftAltHeld() => GetCurrentKeyState().IsKeyDown(Keys.LeftAlt);

        public bool IsKeyHeld(Keys Key) => GetCurrentKeyState().IsKeyDown(Key);
        public bool WasKeyPressed(Keys Key) => GetRolledbackKeyState(1).IsKeyDown(Key) && !GetCurrentKeyState().IsKeyDown(Key);

        public void Rumble(float StartStrength, float EndStrength, float Duration)
        {
            leftRumble = new AutoTween(StartStrength, EndStrength, Duration);
            rightRumble = leftRumble;
            leftRumble.Start();
        }

        public KeyboardState GetCurrentKeyState()
        {
            return keyState[revolvingIndex];
        }
        public GamePadState GetCurrentPadState()
        {
            return padState[revolvingIndex];
        }

        public MouseState GetCurrentMouseState()
        {
            return mouseState[revolvingIndex];
        }

        public KeyboardState GetLastFrameKeyState()
        {
            return GetRolledbackKeyState(1);
        }
        public GamePadState GetLastFramePadState()
        {
            return GetRolledbackPadState(1);
        }

        public MouseState GetLastFrameMouseState()
        {
            return GetRolledbackMouseState(1); ;
        }

        public KeyboardState GetRolledbackKeyState(int Rollback)
        {
            return keyState[(revolvingIndex - Rollback + REVOLVING_BUFFER_SIZE) % REVOLVING_BUFFER_SIZE];
        }
        public GamePadState GetRolledbackPadState(int Rollback)
        {
            return padState[(revolvingIndex - Rollback + REVOLVING_BUFFER_SIZE) % REVOLVING_BUFFER_SIZE];
        }

        public MouseState GetRolledbackMouseState(int Rollback)
        {
            return mouseState[(revolvingIndex - Rollback + REVOLVING_BUFFER_SIZE) % REVOLVING_BUFFER_SIZE];
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IInputService);
        }

        public Vector2 GetMouseInScreenSpace(int Rollback = 0)
        {
            IRenderService rs = GameService.GetService<IRenderService>();
            MouseState mouseState = GetRolledbackMouseState(Rollback);
            return new Vector2(
                mouseState.Position.X / rs.GetTargetToDisplayRatioX(),
                mouseState.Position.Y / rs.GetTargetToDisplayRatioY()
            );
        }

        public Vector2 GetMouseInWorldSpace2()
        {
            IRenderService rs = GameService.GetService<IRenderService>();
            Vector2 cameraPosition = GameService.GetService<ISceneService>().CurrentScene.Camera.GetCenterPoint();
            MouseState mouseState = GetCurrentMouseState();
            return new Vector2(
                mouseState.Position.X / rs.GetTargetToDisplayRatioX() + cameraPosition.X - rs.InternalResolutionX / 2,
                mouseState.Position.Y / rs.GetTargetToDisplayRatioY() + cameraPosition.Y - rs.InternalResolutionY / 2
            );
        }

        public Vector3 GetMouseInWorldSpace3()
        {
            Vector2 worldPosition2 = GetMouseInWorldSpace2();
            List<BaseWorldTile> worldTiles = GameService.GetService<ISceneService>().CurrentScene.GetEntitiesOfTypeSLOW<BaseWorldTile>();
            IEnumerable<BaseWorldTile> orderedWorldTiles = worldTiles.OrderByDescending(wt => wt.BackingBox.Top);
            foreach (BaseWorldTile wt in orderedWorldTiles)
            {
                if (
                    worldPosition2.X >= wt.BackingBox.Left &&
                    worldPosition2.X <= wt.BackingBox.Right &&
                    worldPosition2.Y >= (wt.BackingBox.Back - wt.BackingBox.Top) &&
                    worldPosition2.Y <= (wt.BackingBox.Front - wt.BackingBox.Top)
                )
                {
                    return new Vector3(worldPosition2.X, wt.BackingBox.Top, worldPosition2.Y + wt.BackingBox.Top);
                }
            }
            return new Vector3(worldPosition2.X, 0, worldPosition2.Y);
        }

        public bool IsLeftMouseButtonHeld()
        {
            if (isClickPropagationPaused)
            {
                return false;
            }
            return GetCurrentMouseState().LeftButton == ButtonState.Pressed;
        }

        public bool IsMiddleMouseButtonHeld()
        {
            if (isClickPropagationPaused)
            {
                return false;
            }
            return GetCurrentMouseState().MiddleButton == ButtonState.Pressed;
        }

        public bool IsRightMouseButtonHeld()
        {
            if (isClickPropagationPaused)
            {
                return false;
            }
            return GetCurrentMouseState().RightButton == ButtonState.Pressed;
        }

        public bool WasLeftMouseButtonClicked()
        {
            if (isClickPropagationPaused)
            {
                return false;
            }
            return GetCurrentMouseState().LeftButton == ButtonState.Released && GetLastFrameMouseState().LeftButton == ButtonState.Pressed;
        }

        public bool WasMiddleMouseButtonClicked()
        {
            if (isClickPropagationPaused)
            {
                return false;
            }
            return GetCurrentMouseState().MiddleButton == ButtonState.Released && GetLastFrameMouseState().MiddleButton == ButtonState.Pressed;
        }

        public bool WasRightMouseButtonClicked()
        {
            if (isClickPropagationPaused)
            {
                return false;
            }
            return GetCurrentMouseState().RightButton == ButtonState.Released && GetLastFrameMouseState().RightButton == ButtonState.Pressed;
        }

        public void StopClickPropagation()
        {
            isClickPropagationPaused = true;
        }
    }
}
