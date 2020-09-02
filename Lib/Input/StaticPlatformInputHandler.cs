using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib.Input
{
    public class StaticPlatformInputHandler : IPlatformInputHandler
    {

        public Vector2 GetInputVector()
        {
            return Vector2.Zero;
        }

        public bool ShouldAcceptInput()
        {
            return false;
        }

        public bool WasJumpButtonPressed()
        {
            return false;
        }
    }
}
