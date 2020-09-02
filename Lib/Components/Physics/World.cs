using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ProjectWarnerShared.Lib;
using ProjectWarnerShared.Lib.Components;
using ProjectWarnerShared.Lib.Components.Physics;
using ProjectWarnerShared.Lib.Helpers;
using ProjectWarnerShared.Services;

namespace ProjectWarnerShared
{
    public class World : IWorld
    {
        public const string WORLD_KEY = "world";

        protected List<BackingBox> boxes;

        public World()
        {
            boxes = new List<BackingBox>();
        }

        public BackingBox SpawnStatic(float X, float Y, float Z, float Width, float Height, float Depth)
        {
            BackingBox box = new BackingBox(BackingBox.IType.Static, X, Y, Z, Width, Height, Depth);
            boxes.Add(box);
            return box;
        }

        public BackingBox SpawnFree(float X, float Y, float Z, float Width, float Height, float Depth)
        {
            BackingBox box = new BackingBox(BackingBox.IType.Free, X, Y, Z, Width, Height, Depth);
            boxes.Add(box);
            return box;
        }

        public BackingBox SpawnSticky(float X, float Y, float Z, float Width, float Height, float Depth)
        {
            BackingBox box = new BackingBox(BackingBox.IType.Sticky, X, Y, Z, Width, Height, Depth);
            boxes.Add(box);
            return box;
        }

        public IWorld AddBox(BackingBox B)
        {
            boxes.Add(B);
            boxes.Sort((a, b) => b.Top.CompareTo(a.Top));
            return this;
        }

        public IWorld RemoveBox(BackingBox B)
        {
            boxes.Remove(B);
            return this;
        }

        public Vector3 Move(BackingBox TargetBox, Vector3 PositionDelta)
        {
            switch (TargetBox.InteractionType)
            {
                case BackingBox.IType.Static:
                case BackingBox.IType.Free:
                    return MoveFree(TargetBox, PositionDelta);
                case BackingBox.IType.None:
                    TargetBox.Move(PositionDelta);
                    break;
                case BackingBox.IType.Sticky:
                    return MoveSticky(TargetBox, PositionDelta);
            }
            return Vector3.One;
        }

        protected Vector3 MoveFree(BackingBox TargetBox, Vector3 PositionDelta, bool ShouldHugTerrain = true)
        {
            if (PositionDelta.LengthSquared() == 0)
            {
                return Vector3.One;
            }
            bool isOnGroundPreMove = false;
            if (ShouldHugTerrain && PositionDelta.Y == 0)
            {
                isOnGroundPreMove = IsBoxOnGround(TargetBox);
            }
            TargetBox.Move(PositionDelta);
            if (isOnGroundPreMove)
            {
                if(!IsBoxOnGround(TargetBox))
                {
                    // Ramps are only oriented north south for now
                    TargetBox.Move(new Vector3(0, -Math.Abs(PositionDelta.Z), 0));
                }
            }

            bool hasCollided;
            Vector3 velocityAdjustments = Vector3.One;
            do
            {
                hasCollided = false;
                foreach (BackingBox box in boxes)
                {
                    if (!box.IsInteractive() || TargetBox == box || !TargetBox.B.DoesIntersect(box.B))
                    {
                        continue;
                    }
                    hasCollided = true;
                    float xIntersectRatio = -1;
                    float yIntersectRatio = -1;
                    float zIntersectRatio = -1;
                    if (PositionDelta.X > 0)
                    {
                        xIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Left) / PositionDelta.X
                        );
                    } else if (PositionDelta.X < 0)
                    {
                        xIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Right) / PositionDelta.X
                        );
                    }
                    if (PositionDelta.Y > 0)
                    {
                        yIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Bottom) / PositionDelta.Y
                        );
                    }
                    else if (PositionDelta.Y < 0)
                    {
                        yIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Top) / PositionDelta.Y
                        );
                    }
                    else
                    {
                        yIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Top) / new Vector2(PositionDelta.X, PositionDelta.Z).Length()
                        );
                    }
                    if (PositionDelta.Z > 0)
                    {
                        zIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Back) / PositionDelta.Z
                        );
                    }
                    else if (PositionDelta.Z < 0)
                    {
                        zIntersectRatio = Math.Abs(
                            TargetBox.GetFaceIntersectDistance(box, BackingBox.Face.Front) / PositionDelta.Z
                        );
                    }
                    if (
                        (yIntersectRatio == -1 || (xIntersectRatio != -1 && xIntersectRatio <= yIntersectRatio)) &&
                        (zIntersectRatio == -1 || (xIntersectRatio != -1 && xIntersectRatio <= zIntersectRatio))
                    )
                    {
                        TargetBox.MatchFace(box, PositionDelta.X > 0 ? BackingBox.Face.Left : BackingBox.Face.Right);
                        velocityAdjustments.X = 0;
                    }
                    else if (
                        (xIntersectRatio == -1 || (yIntersectRatio != -1 && yIntersectRatio <= xIntersectRatio)) &&
                        (zIntersectRatio == -1 || (yIntersectRatio != -1 && yIntersectRatio <= zIntersectRatio))
                    )
                    {
                        if (box.B.isRamp)
                        {
                            if (PositionDelta.Z > 0)
                            {
                                velocityAdjustments.Z = 1.25f;
                            }
                            else if (PositionDelta.Z < 0)
                            {
                                velocityAdjustments.Z = 0.75f;
                            }
                        }

                        TargetBox.MatchFace(box, PositionDelta.Y > 0 ? BackingBox.Face.Bottom : BackingBox.Face.Top);
                        velocityAdjustments.Y = 0;
                    }
                    else if (
                        (xIntersectRatio == -1 || (zIntersectRatio != -1 && zIntersectRatio <= xIntersectRatio)) &&
                        (yIntersectRatio == -1 || (zIntersectRatio != -1 && zIntersectRatio <= yIntersectRatio))
                    )
                    {
                        TargetBox.MatchFace(box, PositionDelta.Z > 0 ? BackingBox.Face.Back : BackingBox.Face.Front);
                        velocityAdjustments.Z = 0;
                    }
                }
            } while (hasCollided);

            return velocityAdjustments;
        }

        protected Vector3 MoveSticky(BackingBox TargetBox, Vector3 PositionDelta)
        {
            TargetBox.Move(new Vector3(0, 1, 0));
            foreach (BackingBox box in boxes)
            {
                if (box != TargetBox && TargetBox.B.DoesIntersect(box.B))
                {
                    box.Move(PositionDelta);
                }
            }
            TargetBox.Move(new Vector3(0, -1, 0) + PositionDelta);

            return Vector3.One;
        }

        public bool IsBoxOnGround(BackingBox TargetBox)
        {
            if (TargetBox.InteractionType == BackingBox.IType.None)
            {
                return false;
            }
            TargetBox.Move(new Vector3(0, -1, 0));
            bool isOnGround = false;
            foreach (BackingBox box in boxes)
            {
                if (TargetBox != box && box.IsInteractive() && TargetBox.B.DoesIntersect(box.B))
                {
                    isOnGround = true;
                    break;
                }
            }
            TargetBox.Move(new Vector3(0, 1, 0));
            return isOnGround;
        }

        public bool IsBoxPushingAgainstWall(BackingBox TargetBox, Vector2 Velocity)
        {
            BackingBox offsetTargetBox = new BackingBox(
                BackingBox.IType.None,
                TargetBox.Left + Velocity.X,
                TargetBox.Bottom,
                TargetBox.Back + Velocity.Y,
                TargetBox.Width,
                TargetBox.Height,
                TargetBox.Depth
            );
            foreach (BackingBox box in boxes)
            {
                if (TargetBox != box && box.IsInteractive() && offsetTargetBox.B.DoesIntersect(box.B))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsBoxHangingOverCliff(BackingBox TargetBox, Vector2 Velocity)
        {
            BackingBox offsetTargetBox = new BackingBox(
                BackingBox.IType.None,
                TargetBox.Left + Velocity.X,
                TargetBox.Bottom,
                TargetBox.Back + Velocity.Y,
                TargetBox.Width,
                TargetBox.Height,
                TargetBox.Depth
            );
            List<Vector3> corners = new List<Vector3>
            {
                new Vector3(offsetTargetBox.Left, offsetTargetBox.Bottom - 1, offsetTargetBox.Back),
                new Vector3(offsetTargetBox.Right, offsetTargetBox.Bottom - 1, offsetTargetBox.Back),
                new Vector3(offsetTargetBox.Right, offsetTargetBox.Bottom - 1, offsetTargetBox.Front),
                new Vector3(offsetTargetBox.Left, offsetTargetBox.Bottom - 1, offsetTargetBox.Front),
            };
            foreach (BackingBox box in boxes)
            {
                if (TargetBox == box)
                {
                    continue;
                }
                for (int i = 0; i < corners.Count; )
                {
                    Vector3 corner = corners[i];
                    if (box.B.DoesContainPoint(corner))
                    {
                        corners.RemoveAt(i);
                        continue;
                    }
                    i++;
                }
                if (corners.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsDirectionClearAtDistance(BackingBox B, Enums.Direction Direction, float Distance)
        {
            Box expandedBox = B.B.GetDirectionalExpansion(Direction, Distance, -2, -4);
            foreach (BackingBox b in boxes)
            {
                if (b == B || b.InteractionType != BackingBox.IType.Static)
                {
                    continue;
                }
                if (b.B.DoesIntersect(expandedBox))
                {
                    return false;
                }
            }
            return true;
        }

        public static IWorld AddWorldToCurrentLevel()
        {
            World world = new World();
            GameService.GetService<SceneService>().CurrentScene
                .SetLocalValue(WORLD_KEY, world);
            return world;
        }

        public static IWorld GetWorldFromCurrentLevel()
        {
            return GameService.GetService<SceneService>().CurrentScene.GetLocalValue<IWorld>(WORLD_KEY);
        }
    }
}
