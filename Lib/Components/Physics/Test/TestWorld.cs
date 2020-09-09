using Microsoft.Xna.Framework;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Components.Physics.Test
{
    public class TestWorld : IWorld
    {
        public IWorld AddBox(BackingBox B)
        {
            return this;
        }

        public bool IsBoxOnGround(BackingBox TargetBox)
        {
            return true;
        }

        public bool IsBoxPushingAgainstWall(BackingBox TargetBox, Vector2 Velocity)
        {
            return false;
        }

        public Vector3 Move(BackingBox TargetBox, Vector3 PositionDelta)
        {
            return Vector3.One;
        }

        public bool IsDirectionClearAtDistance(BackingBox B, Enums.Direction Direction, float Distance)
        {
            return true;
        }

        public IWorld RemoveBox(BackingBox B)
        {
            return this;
        }

        public static IWorld AddTestWorldToCurrentScene()
        {
            IWorld world = new TestWorld();
            GameService.GetService<ISceneService>().CurrentScene
                .SetLocalValue(World.WORLD_KEY, world);
            return world;
        }

    }
}
