using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib.Input
{
    public interface IPlatformInputHandler
    {
        bool ShouldAcceptInput();
        Vector2 GetInputVector();
        bool WasJumpButtonPressed();
    }
}
