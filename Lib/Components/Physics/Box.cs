using System;

using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib
{
    public struct Box
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public readonly float Width;
        public readonly float Height;
        public readonly float Depth;

        public readonly float Left;
        public readonly float Right;
        public readonly float Bottom;
        public readonly float Top;
        public readonly float Back;
        public readonly float Front;

        public readonly float MidX;
        public readonly float MidY;
        public readonly float MidZ;

        public readonly Rectangle Projection;

        public readonly bool isTriangle;
        public readonly bool isRamp;

        public readonly float TopDownSurfaceArea;

        public static readonly Box Dummy = new Box(float.MinValue, float.MinValue, float.MinValue, 0, 0, 0);

        public Box(float X, float Y, float Z, float Width, float Height, float Depth, bool IsTriangle = false, bool IsRamp = false)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            if (Width < 0)
            {
                this.X += Width;
            }
            if (Height < 0)
            {
                this.Y += Height;
            }
            if (Depth < 0)
            {
                this.Z += Depth;
            }

            this.Width = Math.Abs(Width);
            this.Height = Math.Abs(Height);
            this.Depth = Math.Abs(Depth);

            Left = this.X;
            Right = this.X + this.Width;
            Bottom = this.Y;
            Top = this.Y + this.Height;
            Back = this.Z;
            Front = this.Z + this.Depth;

            MidX = this.X + this.Width / 2;
            MidY = this.Y + this.Height / 2;
            MidZ = this.Z + this.Depth / 2;

            Projection = new Rectangle(
                (int)Left, 
                (int)(Front - this.Height - Bottom - this.Depth), 
                (int)this.Width, 
                (int)(this.Height + this.Depth)
            );

            isTriangle = IsTriangle;
            isRamp = IsRamp;

            TopDownSurfaceArea = this.Width * this.Depth;

            if (Width == 32 && Height == 16 && Depth == 32)
            {
                isRamp = true;
            }
        }

        public int Compare(Box OtherBox)
        {
            if (Bottom >= OtherBox.Top)
            {
                return 1;
            }
            else if (OtherBox.Bottom >= Top)
            {
                return -1;
            }
            else if (Back >= OtherBox.Front)
            {
                return 1;
            }
            else if (OtherBox.Back >= Front)
            {
                return -1;
            }
            else
            {
                if (Top > OtherBox.Top)
                {
                    var otherBoxY = OtherBox.Top - OtherBox.Height / OtherBox.Depth * (Front - OtherBox.Front);
                    if (Top >= otherBoxY)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    var boxY = Top - Height / Depth * (OtherBox.Front - Front);
                    if (OtherBox.Top >= boxY)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }

        public Vector3 GetPosition()
        {
            return new Vector3(X, Y, Z);
        }

        public Vector3 GetBottomCenterPoint()
        {
            return new Vector3(
                X + Width / 2,
                Y,
                Z + Depth / 2
            );
        }

        public bool DoesContainPoint(Vector3 P)
        {
            return (
                P.X > Left &&
                P.X < Right &&
                P.Y > Bottom &&
                P.Y < Top &&
                P.Z > Back &&
                P.Z < Front
            );
        }

        public bool DoesContainPointEdgeInclusive(Vector3 P)
        {
            return (
                P.X >= Left &&
                P.X <= Right &&
                P.Y >= Bottom &&
                P.Y <= Top &&
                P.Z >= Back &&
                P.Z <= Front
            );
        }

        public bool DoesIntersectPlane(float X1, float Z, float X2)
        {
            return (
                X2 > Left &&
                X1 < Right &&
                Z > Back &&
                Z < Front
            );
        }

        public bool DoesIntersect(Box Box2)
        {
            bool doBoxesIntersect = (
                Right > Box2.Left &&
                Left < Box2.Right &&
                Top > Box2.Bottom &&
                Bottom < Box2.Top &&
                Front > Box2.Back &&
                Back < Box2.Front
            );
            if (!doBoxesIntersect)
            {
                return false;
            }
            else if ((!isTriangle && !Box2.isTriangle) && (!isRamp && !Box2.isRamp))
            {
                return true;
            }
            else if (isRamp && !Box2.isRamp)
            {
                return (Box2.Bottom - Bottom) < (Front - Box2.Back) * (Height / Depth);
            }
            else if (!isRamp && Box2.isRamp)
            {
                return (Bottom - Box2.Bottom) < (Box2.Front - Back) * (Box2.Height / Box2.Depth);
            }
            else if (isTriangle && !Box2.isTriangle)
            {
                return (Box2.Left - Left) < (Front - Box2.Back);
            }
            else if (!isTriangle && Box2.isTriangle)
            {
                return (Left - Box2.Left) < (Box2.Front - Back);
            }
            throw new Exception("Can't check intersection between two triangles");
        }

        public bool DoesContain(Box Box2)
        {
            return (
                Box2.Left >= Left &&
                Box2.Right <= Right &&
                Box2.Bottom >= Bottom &&
                Box2.Top <= Top &&
                Box2.Back >= Back &&
                Box2.Front <= Front
            );
        }

        public Box? GetIntersectionVolume(Box Box2)
        {
            if (!DoesIntersect(Box2))
            {
                return null;
            }
            float left = Math.Max(Left, Box2.Left);
            float right = Math.Min(Right, Box2.Right);
            float back = Math.Max(Back, Box2.Back);
            float front = Math.Min(Front, Box2.Front);
            float bottom = Math.Max(Bottom, Box2.Bottom);
            float top = Math.Min(Top, Box2.Top);

            return new Box(
                left,
                bottom,
                back,
                right - left,
                top - bottom,
                front - back
            );
        }

        public bool DoesProjectionOverlapOther(Box Other)
        {
            Rectangle p = Projection;
            Rectangle po = Other.Projection;
            return p.Intersects(po);
        }

        public Vector3 GetCenterPoint()
        {
            return new Vector3(X + Width / 2, Y + Height / 2, Z + Depth / 2);
        }

        public Box GetInflatedBox(float XWise, float YWise, float ZWise)
        {
            return new Box(
                X - XWise,
                Y - YWise,
                Z - ZWise,
                Width + XWise * 2,
                Height + YWise * 2,
                Depth + ZWise * 2
            );
        }

        public Box GetDirectionalExpansion(Enums.Direction Direction, float Parallel, float Perpendicular, float Axis)
        {
            switch (Direction)
            {
                case Enums.Direction.up:
                    return new Box(
                        X - Perpendicular / 2,
                        Y - Axis / 2,
                        Z - Parallel,
                        Width + Perpendicular,
                        Height + Axis,
                        Depth + Parallel
                    );
                case Enums.Direction.right:
                    return new Box(
                        X,
                        Y - Axis / 2,
                        Z - Perpendicular / 2,
                        Width + Parallel,
                        Height + Axis,
                        Depth + Perpendicular
                    );
                case Enums.Direction.down:
                    return new Box(
                        X - Perpendicular / 2,
                        Y - Axis / 2,
                        Z,
                        Width + Perpendicular,
                        Height + Axis,
                        Depth + Parallel
                    );
                case Enums.Direction.left:
                    return new Box(
                        X - Parallel,
                        Y - Axis / 2,
                        Z - Perpendicular / 2,
                        Width + Parallel,
                        Height + Axis,
                        Depth + Perpendicular
                    );
                default:
                    throw new Exception("Invalid direction provide");
            }
        }

        public Box GetTranslation(Vector3 Translation)
        {
            return new Box(X + Translation.X, Y + Translation.Y, Z + Translation.Z, Width, Height, Depth);
        }

        public Rectangle GetTopDownRectangle()
        {
            return new Rectangle(
                (int)Left,
                (int)(Back - Height - Bottom),
                (int)Width,
                (int)(Depth + Height)
            );
        }
    }
}
