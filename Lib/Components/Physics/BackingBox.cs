using System;

using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib.Components
{
    [Serializable]
    public class BackingBox : IPositionableElement3, IComponent
    {
        private static ObjectPool<BackingBox> boxPool;

        public static readonly BackingBox Dummy = new BackingBox(IType.None, 0, 0, 0, 0, 0, 0);
        public enum IType { Static, Free, Sticky, None }
        public enum Face { Left, Right, Top, Bottom, Back, Front };
        public enum SortingMode { Box, Front }

        public IType InteractionType { get; set; }

        public Box B;
        public SortingMode SortMode { get; private set; }

        public float X => B.X;
        public float Y => B.Y;
        public float Z => B.Z;

        public float Width => B.Width;
        public float Height => B.Height;
        public float Depth => B.Depth;
        public float ProjectedHeight => Height + Depth;

        public float Left => B.Left;
        public float Right => B.Right;
        public float Bottom => B.Bottom;
        public float Top => B.Top;
        public float Back => B.Back;
        public float Front => B.Front;

        public float MidX => B.MidX;
        public float MidY => B.MidY;
        public float MidZ => B.MidZ;

        static BackingBox()
        {
            boxPool = new ObjectPool<BackingBox>(500);
        }

        public BackingBox() { }

        public static BackingBox Build(IType InteractionType, float X, float Y, float Z, float Width, float Height, float Depth, bool IsTriangle = false, bool IsRamp = false, SortingMode SortMode = SortingMode.Front)
        {
            BackingBox b = boxPool.Rent();
            b.InteractionType = InteractionType;
            b.B = new Box(X, Y, Z, Width, Height, Depth, IsTriangle, IsRamp);
            b.SortMode = SortMode;
            return b;
        }

        public static BackingBox Build(IType InteractionType, Box B, SortingMode SortMode = SortingMode.Box)
        {
            BackingBox b = boxPool.Rent();
            b.InteractionType = InteractionType;
            b.B = B;
            b.SortMode = SortMode;
            return b;
        }

        public static void Return(BackingBox B)
        {
            boxPool.Return(B);
        }

        public BackingBox(IType InteractionType, float X, float Y, float Z, float Width, float Height, float Depth, bool IsTriangle = false, bool IsRamp = false, SortingMode SortMode = SortingMode.Front)
        {
            this.InteractionType = InteractionType;
            B = new Box(X, Y, Z, Width, Height, Depth, IsTriangle, IsRamp);
            this.SortMode = SortMode;
        }

        public BackingBox(IType InteractionType, Box B, SortingMode SortMode = SortingMode.Box)
        {
            this.InteractionType = InteractionType;
            this.B = B;
            this.SortMode = SortMode;
        }

        public static BackingBox CreateBackingBoxFromBounds(IType InteractionType, float Left, float Bottom, float Back, float Right, float Top, float Front, SortingMode SortMode = SortingMode.Front)
        {
            return new BackingBox(
                InteractionType,
                Left,
                Bottom,
                Back,
                Right - Left,
                Top - Bottom,
                Front - Back,
                SortMode: SortMode
            );
        }

        public void PreDraw(float DT) { }

        public void PostDraw() { }

        public int Compare(BackingBox OtherBox)
        {
            if (this == OtherBox)
            {
                return 0;
            }
            if (this == Dummy)
            {
                return 1;
            }
            else if (OtherBox == Dummy)
            {
                return -1;
            }
            return B.Compare(OtherBox.B);
        }

        public virtual void Teleport(Vector3 Position)
        {
            B = new Box(Position.X, Position.Y, Position.Z, B.Width, B.Height, B.Depth);
        }

        public void TeleportCentered(Vector3 Position)
        {
            Teleport(Position - new Vector3(B.Width / 2, B.Height / 2, B.Depth / 2));
        }

        public void Move(Vector3 PositionDelta)
        {
            Teleport(new Vector3(B.X, B.Y, B.Z) + PositionDelta);
        }

        public void MatchFace(BackingBox Box2, Face ContactFace, Vector3? Offset = null)
        {
            float x = X;
            float y = Y;
            float z = Z;
            switch (ContactFace)
            {
                case Face.Left:
                    x = Box2.Left - Width;
                    break;
                case Face.Right:
                    x = Box2.Right;
                    break;
                case Face.Bottom:
                    y = Box2.Bottom - Height;
                    break;
                case Face.Top:
                    if (Box2.B.isRamp)
                    {
                        y = (Box2.Front - Back) * (Box2.Height / Box2.Depth) + Box2.Bottom;
                    }
                    else
                    {
                        y = Box2.Top;
                    }
                    break;
                case Face.Back:
                    z = Box2.Back - Depth;
                    break;
                case Face.Front:
                    z = Box2.Front;
                    break;
            }
            Vector3 offset = Offset.HasValue ? Offset.Value : Vector3.Zero;
            Teleport(new Vector3(x, y, z) + offset);
        }

        public float GetFaceIntersectDistance(BackingBox Box2, Face ContactFace)
        {
            switch (ContactFace)
            {
                case Face.Left:
                    return Right - Box2.Left;
                case Face.Right:
                    return Box2.Right - Left;
                case Face.Bottom:
                    return Top - Box2.Bottom;
                case Face.Top:
                    if (Box2.B.isRamp)
                    {
                        return (Box2.Front - Back) * (Box2.Height / Box2.Depth) - (Bottom - Box2.Bottom);
                    }
                    return Box2.Top - Bottom;
                case Face.Back:
                    return Front - Box2.Back;
                case Face.Front:
                    return Box2.Front - Back;
                default:
                    return 0;
            }
        }

        public Vector3 GetPosition()
        {
            return new Vector3(X, Y, Z);
        }

        public Vector3 GetCenterPoint()
        {
            return new Vector3(X + Width / 2, Y + Height / 2, Z + Depth / 2);
        }

        public Vector3 GetBottomCenterPoint()
        {
            return new Vector3(X + Width / 2, Y, Z + Depth / 2);
        }

        public Vector2 GetProjectedPosition2()
        {
            return new Vector2(X, Front - Height - Bottom);
        }

        public Vector2 GetProjectedCenterPoint2()
        {
            return new Vector2(X + Width / 2, Front - Height / 2 - Bottom);
        }

        public Vector2 GetFullProjectedPosition2()
        {
            return new Vector2(Left, Front - Height - Depth - Bottom);
        }

        public Rectangle GetBoundingRectangle()
        {
            Vector2 projectedPosition = GetFullProjectedPosition2();
            return new Rectangle((int)projectedPosition.X, (int)projectedPosition.Y, (int)Width, (int)(Height + Depth));
        }

        public bool DoesProjectionOverlapOther(BackingBox Other)
        {
            if (this == Dummy || Other == Dummy)
            {
                return true;
            }
            return B.Projection.Intersects(Other.B.Projection);
        }

        public bool DoesProjectionOverlapPoint(Vector2 Other)
        {
            Vector2 p = GetFullProjectedPosition2();
            return new Rectangle((int)p.X, (int)p.Y, (int)Width, (int)Height + (int)Depth).Contains((int)Other.X, (int)Other.Y);
        }

        public bool IsInteractive()
        {
            return InteractionType != IType.None && InteractionType != IType.Free;
        }
    }
}
