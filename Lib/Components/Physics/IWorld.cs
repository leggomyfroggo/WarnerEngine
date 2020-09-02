using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib.Components.Physics
{
    public interface IWorld
    {
        IWorld AddBox(BackingBox B);
        IWorld RemoveBox(BackingBox B);
        Vector3 Move(BackingBox TargetBox, Vector3 PositionDelta);
        bool IsBoxOnGround(BackingBox TargetBox);
        bool IsBoxPushingAgainstWall(BackingBox TargetBox, Vector2 Velocity);
        bool IsDirectionClearAtDistance(BackingBox B, Enums.Direction Direction, float Distance);
    }
}
