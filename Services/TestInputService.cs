using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public class TestInputService : IInputService
    {
        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize() { }

        public void PreDraw(float DT)
        {
            throw new NotImplementedException();
        }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public KeyboardState GetCurrentKeyState()
        {
            throw new NotImplementedException();
        }

        public GamePadState GetCurrentPadState()
        {
            throw new NotImplementedException();
        }

        public KeyboardState GetLastFrameKeyState()
        {
            throw new NotImplementedException();
        }

        public GamePadState GetLastFramePadState()
        {
            throw new NotImplementedException();
        }

        public Enums.Direction? GetLeftStickTapDirection()
        {
            return null;
        }

        public Vector2 GetLeftStickVector(int Rollback = 0)
        {
            throw new NotImplementedException();
        }

        public KeyboardState GetRolledbackKeyState(int Rollback)
        {
            throw new NotImplementedException();
        }

        public GamePadState GetRolledbackPadState(int Rollback)
        {
            throw new NotImplementedException();
        }

        public bool IsActionHeld(InputAction Action)
        {
            throw new NotImplementedException();
        }

        public bool IsLeftAltHeld()
        {
            throw new NotImplementedException();
        }

        public bool IsLeftShiftHeld()
        {
            throw new NotImplementedException();
        }

        public bool IsKeyHeld(Keys Key)
        {
            return false;
        }

        public bool WasKeyPressed(Keys Key)
        {
            return false;
        }

        public IInputService RegisterInput(InputAction Action, Keys? Key = null, Buttons? Button = null)
        {
            throw new NotImplementedException();
        }

        public void Rumble(float StartStrength, float EndStrength, float Duration) { }

        public bool WasActionDoubleTapped(InputAction Action)
        {
            return false;
        }

        public Enums.Direction? GetDirectionDoubleTapped()
        {
            return null;
        }

        public bool WasActionPressed(InputAction Action, bool Consume = false)
        {
            throw new NotImplementedException();
        }

        public bool WasActionTapped(InputAction Action)
        {
            throw new NotImplementedException();
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IInputService);
        }

        public MouseState GetCurrentMouseState()
        {
            throw new NotImplementedException();
        }

        public MouseState GetLastFrameMouseState()
        {
            throw new NotImplementedException();
        }

        public MouseState GetRolledbackMouseState(int Rollback)
        {
            throw new NotImplementedException();
        }

        public Vector2 GetMouseInScreenSpace(int Rollback = 0)
        {
            throw new NotImplementedException();
        }

        public Vector2 GetMouseInWorldSpace2()
        {
            throw new NotImplementedException();
        }

        public Vector3 GetMouseInWorldSpace3()
        {
            throw new NotImplementedException();
        }

        public bool IsLeftMouseButtonHeld()
        {
            throw new NotImplementedException();
        }

        public bool WasLeftMouseButtonClicked()
        {
            throw new NotImplementedException();
        }

        public bool IsMiddleMouseButtonHeld()
        {
            throw new NotImplementedException();
        }

        public bool IsRightMouseButtonHeld()
        {
            throw new NotImplementedException();
        }

        public bool WasMiddleMouseButtonClicked()
        {
            throw new NotImplementedException();
        }

        public bool WasRightMouseButtonClicked()
        {
            throw new NotImplementedException();
        }

        public void StopClickPropagation()
        {
            throw new NotImplementedException();
        }
    }
}
