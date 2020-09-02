using System;

using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Input;
using WarnerEngine.Lib.Components.Physics;

namespace WarnerEngine.Lib.Components
{
    public class PlatformMovement
    {
        public enum HitGroundBehavior { None, Bounce };

        protected BackingBox BackingBox { get; set; }

        protected IPlatformInputHandler InputHandler { get; set; }

        private float maxSpeed;
        protected float MaxSpeed {
            get
            {
                return maxSpeed * MaxVelocityScaler;
            }
            set
            {
                maxSpeed = value;
            }
        }

        protected int MinSpeed { get; set; }

        protected int JumpStrength { get; set; }

        protected int GravityStrength { get; set; }

        private float acceleration;
        protected float Acceleration
        {
            get
            {
                return acceleration * AccelerationScaler;
            }
            set
            {
                acceleration = value;
            }
        }

        private float decceleration;
        protected float Decceleration
        {
            get
            {
                return decceleration * DeccelerationScaler;
            }
            set
            {
                decceleration = value;
            }
        }

        protected int AirAcceleration { get; set; }

        protected int AirDecceleration { get; set; }

        public float MaxVelocityScaler { get; set; }

        public float AccelerationScaler { get; set; }

        public float DeccelerationScaler { get; set; }

        public float JumpStrengthScaler { get; set; }

        public Vector3 VelocityScaler { get; set; }

        public Vector3 Velocity { get; private set; }

        protected HitGroundBehavior hitGroundBehavior;

        public static PlatformMovement GetStaticPlatformMovement(BackingBox B, HitGroundBehavior HGB = HitGroundBehavior.None)
        {
            return new PlatformMovement(B, new StaticPlatformInputHandler(), 1000, 30, 0, 1000, 0, 3000, 0, 0, HGB);
        }

        public PlatformMovement(
            BackingBox BackingBox,
            IPlatformInputHandler InputHandler,
            int MaxSpeed, 
            int MinSpeed, 
            int JumpStrength, 
            int GravityStrength, 
            int Acceleration, 
            int Decceleration, 
            int AirAcceleration, 
            int AirDecceleration,
            HitGroundBehavior HGB = HitGroundBehavior.None
        )
        {
            this.BackingBox = BackingBox;
            this.InputHandler = InputHandler;
            this.MaxSpeed = MaxSpeed;
            this.MinSpeed = MinSpeed;
            this.JumpStrength = JumpStrength;
            this.GravityStrength = GravityStrength;
            this.Acceleration = Acceleration;
            this.Decceleration = Decceleration;
            this.AirAcceleration = AirAcceleration;
            this.AirDecceleration = AirDecceleration;
            MaxVelocityScaler = 1f;
            DeccelerationScaler = 1f;
            AccelerationScaler = 1f;
            JumpStrengthScaler = 1f;
            VelocityScaler = Vector3.One;
            hitGroundBehavior = HGB;
        }

        public void PreDraw(float DT)
        {
            IWorld world = World.GetWorldFromCurrentLevel();

            bool isOnGround = world.IsBoxOnGround(BackingBox);
            Vector3 newVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
            Vector3 acceleration = Vector3.Zero;

            if (InputHandler.ShouldAcceptInput())
            {
                Vector2 accelerationVector = InputHandler.GetInputVector();
                acceleration.X = accelerationVector.X;
                acceleration.Z = accelerationVector.Y;
            }

            if (acceleration.Length() > 0)
            {
                Vector3 aNormal = Vector3.Normalize(acceleration);
                newVelocity += aNormal * (isOnGround ? Acceleration : AirAcceleration) * DT;
            }

            if (newVelocity.Length() > 0)
            {
                Vector3 decceleration = Vector3.Normalize(newVelocity) * -1 * (isOnGround ? Decceleration : AirDecceleration);
                newVelocity += decceleration * DT;
            }

            float adjustedMaxSpeed = GetMaxSpeedAdjustedForInput();
            if (newVelocity.Length() > adjustedMaxSpeed)
            {
                newVelocity = Vector3.Normalize(newVelocity) * adjustedMaxSpeed;
            }
            else if (acceleration.Length() == 0 && newVelocity.Length() < MinSpeed)
            {
                newVelocity = Vector3.Zero;
            }

            if (isOnGround && Velocity.Y == 0)
            {
                if (InputHandler.WasJumpButtonPressed() && JumpStrengthScaler > 0)
                {
                    newVelocity.Y = JumpStrength * JumpStrengthScaler;
                }
            }
            else
            {
                newVelocity.Y = Velocity.Y - GravityStrength * DT;
            }

            Velocity = newVelocity;
            Vector3 positionDelta = Velocity * VelocityScaler * DT;
            if (positionDelta.Length() > 0)
            {
                Vector3 velocityCoefficients = world.Move(BackingBox, positionDelta);
                if (hitGroundBehavior == HitGroundBehavior.Bounce && velocityCoefficients.Y == 0 && Math.Abs(Velocity.Y) > 1)
                {
                    velocityCoefficients = new Vector3(0.7f, -0.7f, 0.7f);
                }
                Velocity *= velocityCoefficients;
            }
        }

        private float GetMaxSpeedAdjustedForInput()
        {
            Vector2 inputVector = InputHandler.GetInputVector();
            return inputVector.Length() > 0f
                ? MaxSpeed * InputHandler.GetInputVector().Length()
                : MaxSpeed;
        }

        public bool IsOnGround()
        {
            return World.GetWorldFromCurrentLevel().IsBoxOnGround(BackingBox);
        }

        public void StopMotion()
        {
            Velocity = Vector3.Zero;
        }

        public void ImpartForce(Vector3 Force, bool ProvideInitialKick = true)
        {
            IWorld world = World.GetWorldFromCurrentLevel();
            Velocity += Force;
            if (ProvideInitialKick)
            {
                world.Move(BackingBox, Force * 0.01667f);
            }
        }

        public void ScaleVelocity(Vector3 Scale)
        {
            Velocity *= Scale;
        }
    }
}
