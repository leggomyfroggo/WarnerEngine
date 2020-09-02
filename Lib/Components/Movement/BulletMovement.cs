using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Components.Physics;

namespace WarnerEngine.Lib.Components
{
    public class BulletMovement
    {
        public BackingBox BackingBox { get; private set; }

        public Vector3 Velocity { get; private set; }

        public bool IsGravityEnabled { get; private set; }

        public BulletMovement(BackingBox BackingBox, Vector3 Velocity, bool IsGravityEnabled = false)
        {
            this.BackingBox = BackingBox;
            this.Velocity = Velocity;
            this.IsGravityEnabled = IsGravityEnabled;
            World.GetWorldFromCurrentLevel().AddBox(this.BackingBox);
        }

        public void PreDraw(float DT)
        {
            if (IsGravityEnabled)
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y - 800 * DT, Velocity.Z);
            }
            IWorld world = World.GetWorldFromCurrentLevel();
            world.Move(BackingBox, Velocity * DT);
        }

        public BulletMovement SetVelocity(Vector3 Velocity)
        {
            this.Velocity = Velocity;
            return this;
        }

        public BulletMovement ToggleGravity(bool ShouldEnableGravity)
        {
            IsGravityEnabled = ShouldEnableGravity;
            return this;
        }
    }
}
