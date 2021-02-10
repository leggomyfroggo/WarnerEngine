using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public interface IInputService : IService
    {
        IInputService RegisterInput(InputAction Action, Keys? Key = null, Buttons? Button = null);
        bool IsActionHeld(InputAction Action);
        bool WasActionPressed(InputAction Action, bool Consume = false);
        bool WasActionDoubleTapped(InputAction Action);
        Enums.Direction? GetDirectionDoubleTapped();
        bool WasActionTapped(InputAction Action);
        Vector2 GetLeftStickVector(int Rollback = 0);
        Enums.Direction? GetLeftStickTapDirection();

        bool IsLeftShiftHeld();
        bool IsLeftAltHeld();

        bool IsKeyHeld(Keys Key);
        bool WasKeyPressed(Keys Key);

        void Rumble(float StartStrength, float EndStrength, float Duration);

        KeyboardState GetCurrentKeyState();
        GamePadState GetCurrentPadState();
        MouseState GetCurrentMouseState();

        KeyboardState GetLastFrameKeyState();
        GamePadState GetLastFramePadState();
        MouseState GetLastFrameMouseState();

        KeyboardState GetRolledbackKeyState(int Rollback);
        GamePadState GetRolledbackPadState(int Rollback);
        MouseState GetRolledbackMouseState(int Rollback);

        Vector2 GetMouseInScreenSpace(int Rollback = 0);
        Vector2 GetMouseInWorldSpace2();
        Vector3 GetMouseInWorldSpace3();
        int GetMouseScroll();
        bool IsLeftMouseButtonHeld();
        bool IsMiddleMouseButtonHeld();
        bool IsRightMouseButtonHeld();
        bool WasLeftMouseButtonClicked();
        bool WasMiddleMouseButtonClicked();
        bool WasRightMouseButtonClicked();
        void StopClickPropagation();
    }
}
