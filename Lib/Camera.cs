using System;

using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public class Camera : ISceneEntity, IPreDraw, IPositionableElement2
    {
        private Vector2 position;

        public BackingBox Target { get; set; }

        protected float speed;

        protected AutoTween shakeTimer;
        protected float maxShakeStrength;
        protected float shakeFrequency;

        public Vector2 Offset { get; set; }

        public float Left => position.X - GameService.GetService<RenderService>().InternalResolutionX / 2;
        public float Right => position.X + GameService.GetService<RenderService>().InternalResolutionX / 2;
        public float Top => position.Y - GameService.GetService<RenderService>().InternalResolutionY / 2;
        public float Bottom => position.Y + GameService.GetService<RenderService>().InternalResolutionY / 2;

        public Rectangle? Limits;

        public Camera(Vector2 StartPosition, BackingBox T = null, float Speed = 1)
        {
            position = StartPosition;
            Target = T;
            speed = Speed;
            Limits = null;
            shakeTimer = null;
        }

        public void OnAdd(Scene ParentScene) { }

        public void OnRemove(Scene ParentScene) { }

        public void PreDraw(float DT)
        {
            if (shakeTimer != null && shakeTimer.IsRunning)
            {
                shakeTimer.Update();
            }
            if (Target != null)
            {
                position = Vector2.Lerp(position, Target.GetProjectedCenterPoint2() + Offset, speed);
                if (Limits.HasValue)
                {
                    Rectangle limits = Limits.Value;
                    if (Left < limits.Left) {
                        position.X = limits.Left + GameService.GetService<RenderService>().InternalResolutionX / 2;
                    }
                    if (Right > limits.Right)
                    {
                        position.X = limits.Right - GameService.GetService<RenderService>().InternalResolutionX / 2;
                    }
                    if (Top < limits.Top)
                    {
                        position.Y = limits.Top + GameService.GetService<RenderService>().InternalResolutionY / 2;
                    }
                    if (Bottom > limits.Bottom)
                    {
                        position.Y = limits.Bottom - GameService.GetService<RenderService>().InternalResolutionY / 2;
                    }
                }
            }
        }

        public bool ContainsBox(Box B)
        {
            return (
                B.Right > Left &&
                B.Left < Right &&
                (B.Front - B.Bottom) > Top &&
                (B.Back - B.Height - B.Bottom) < Bottom
            );
        }

        public void StartShake(float MaxShakeStrength, float ShakeDuration, float ShakeFrequency)
        {
            shakeTimer = new AutoTween(0, 1, ShakeDuration);
            shakeTimer.Start();
            maxShakeStrength = MaxShakeStrength;
            shakeFrequency = ShakeFrequency;
        }

        public BackingBox GetBackingBox()
        {
            return null;
        }

        public Vector2 GetCenterPoint()
        {
            return GetPosition();
        }

        public Vector2 GetPosition()
        {
            Vector2 adjustedPosition = position;
            if (shakeTimer != null && shakeTimer.IsRunning)
            {
                float currentTween = shakeTimer.GetTween();
                float periodX = (float)Math.Sin((currentTween * shakeFrequency * shakeTimer.Duration) / 1000 * MathHelper.TwoPi);
                float periodY = (float)Math.Sin((currentTween * shakeFrequency * shakeTimer.Duration) / 1000 * MathHelper.TwoPi + MathHelper.PiOver4);
                adjustedPosition.X += maxShakeStrength * periodX * (1 - currentTween);
                adjustedPosition.Y += (maxShakeStrength / 2) * periodY * (1 - currentTween);
            }
            return adjustedPosition;
        }
    }
}
