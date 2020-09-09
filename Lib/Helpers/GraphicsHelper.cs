using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Helpers
{
    public class GraphicsHelper
    {
        public static Rectangle GetSheetCell(int X, int Y, int TileWidth, int TileHeight, Index2? Offset = null)
        {
            Index2 offset = Offset.HasValue ? Offset.Value : Index2.Zero;
            return new Rectangle(offset.X + X * TileWidth, offset.Y + Y * TileHeight, TileWidth, TileHeight);
        }

        public static Rectangle GetSheetCell(Index2 Index, int TileWidth, int TileHeight, Index2? Offset = null)
        {
            return GetSheetCell(Index.X, Index.Y, TileWidth, TileHeight, Offset);
        }

        public static Vector2 FlattenVector3(Vector3 V)
        {
            return new Vector2(V.X, V.Z);
        }

        public static Enums.Direction RotateDirectionClockwise(Enums.Direction Direction)
        {
            switch (Direction)
            {
                case Enums.Direction.down:
                    return Enums.Direction.left;
                case Enums.Direction.left:
                    return Enums.Direction.up;
                case Enums.Direction.up:
                    return Enums.Direction.right;
                case Enums.Direction.right:
                    return Enums.Direction.down;
            }
            throw new Exception("Invalid direction given");
        }

        public static Enums.Direction RotateDirectionCounterClockwise(Enums.Direction Direction)
        {
            switch (Direction)
            {
                case Enums.Direction.down:
                    return Enums.Direction.right;
                case Enums.Direction.right:
                    return Enums.Direction.up;
                case Enums.Direction.up:
                    return Enums.Direction.left;
                case Enums.Direction.left:
                    return Enums.Direction.down;
            }
            throw new Exception("Invalid direction given");
        }

        public static bool IsTargetLeftOfHeading(Vector3 Origin, Enums.Direction Heading, Vector3 Target)
        {
            switch (Heading)
            {
                case Enums.Direction.down:
                    return Target.X > Origin.X;
                case Enums.Direction.right:
                    return Target.Z <= Origin.Z;
                case Enums.Direction.up:
                    return Target.X <= Origin.X;
                case Enums.Direction.left:
                    return Target.Z >= Origin.Z;
            }
            throw new Exception("Invalid direction given");
        }


        public static Enums.Direction GetDirectionFromVector(Vector2 Vector)
        {
            Vector2 rotatedVector = Vector2.Transform(Vector, Matrix.CreateRotationZ(MathHelper.PiOver4));
            if (rotatedVector.X >= 0 && rotatedVector.Y <= 0)
            {
                return Enums.Direction.up;
            }
            else if (rotatedVector.X <= 0 && rotatedVector.Y >= 0)
            {
                return Enums.Direction.down;
            }
            else if (rotatedVector.X >= 0 && rotatedVector.Y >= 0)
            {
                return Enums.Direction.right;
            }
            else
            {
                return Enums.Direction.left;
            }
        }

        public static Enums.Direction GetDirectionFromVector3ToVector3(Vector3 V1, Vector3 V2)
        {
            return GetDirectionFromVector(FlattenVector3(V2 - V1));
        }

        public static Vector3 GetVector3FromDirection(Enums.Direction Direction)
        {
            switch (Direction)
            {
                case Enums.Direction.down:
                    return new Vector3(0, 0, 1);
                case Enums.Direction.left:
                    return new Vector3(-1, 0, 0);
                case Enums.Direction.up:
                    return new Vector3(0, 0, -1);
                case Enums.Direction.right:
                    return new Vector3(1, 0, 0);
            }
            throw new System.Exception("Invalid direction given");
        }

        public static Vector2 GetVector2FromDirection(Enums.Direction Direction)
        {
            switch (Direction)
            {
                case Enums.Direction.down:
                    return new Vector2(0, 1);
                case Enums.Direction.left:
                    return new Vector2(-1, 0);
                case Enums.Direction.up:
                    return new Vector2(0, -1);
                case Enums.Direction.right:
                    return new Vector2(1, 0);
            }
            throw new System.Exception("Invalid direction given");
        }

        public static Enums.Direction GetOppositeDirection(Enums.Direction Direction)
        {
            switch (Direction)
            {
                case Enums.Direction.down:
                    return Enums.Direction.up;
                case Enums.Direction.up:
                    return Enums.Direction.down;
                case Enums.Direction.left:
                    return Enums.Direction.right;
                case Enums.Direction.right:
                    return Enums.Direction.left;
            }
            throw new System.Exception("Invalid direction given");
        }

        public static bool AreDirectionsOpposites(Enums.Direction Direction1, Enums.Direction Direction2)
        {
            return Direction1 == GetOppositeDirection(Direction2);
        }

        public static Components.BackingBox.Face GetOppositeFaceFromDirection(Enums.Direction Direction)
        {
            switch (Direction)
            {
                case Enums.Direction.down:
                    return Components.BackingBox.Face.Back;
                case Enums.Direction.up:
                    return Components.BackingBox.Face.Front;
                case Enums.Direction.left:
                    return Components.BackingBox.Face.Right;
                case Enums.Direction.right:
                    return Components.BackingBox.Face.Left;
            }
            throw new Exception("Invalid direction given");
        }

        public static void DrawSquare(Rectangle Rect, Color Tint, bool IsFilled = false)
        {
            DrawSquare(Rect.X, Rect.Y, Rect.Width, Rect.Height, Tint, IsFilled);
        }

        public static void DrawSquare(int X, int Y, int W, int H, Color Tint, bool IsFilled = false)
        {
            Texture2D whiteTileTexture = GameService.GetService<IContentService>().GetWhiteTileTexture();
            if (IsFilled)
            {
                GameService.GetService<RenderService>().DrawQuad(
                    whiteTileTexture,
                    new Rectangle(X, Y, W, H),
                    new Rectangle(0, 0, 8, 8),
                    Tint
                );
                return;
            }
            // Top line
            GameService.GetService<RenderService>().DrawQuad(whiteTileTexture, new Rectangle(X, Y, W, 1), new Rectangle(0, 0, 8, 8), Tint);
            // Right line
            GameService.GetService<RenderService>().DrawQuad(whiteTileTexture, new Rectangle(X + W - 1, Y, 1, H), new Rectangle(0, 0, 8, 8), Tint);
            // Bottom line
            GameService.GetService<RenderService>().DrawQuad(whiteTileTexture, new Rectangle(X, Y + H - 1, W, 1), new Rectangle(0, 0, 8, 8), Tint);
            // Left line
            GameService.GetService<RenderService>().DrawQuad(whiteTileTexture, new Rectangle(X, Y, 1, H), new Rectangle(0, 0, 8, 8), Tint);
        }

        public static void DrawCube(int X, int Y, int Z, int W, int H, int D, Color Tint)
        {
            // Bottom square
            DrawSquare(X, Z - Y, W, D, Tint);
            // Top square
            DrawSquare(X, Z - Y - H, W, D, Tint * 0.5f);
        }

        public static void DrawCube(Box B, Color Tint)
        {
            DrawCube((int)B.X, (int)B.Y, (int)B.Z, (int)B.Width, (int)B.Height, (int)B.Depth, Tint);
        }

        public static void DrawLine(int X1, int Y1, int X2, int Y2, Color Tint)
        {
            Texture2D whiteTileTexture = GameService.GetService<IContentService>().GetWhiteTileTexture();
            Vector2 v1 = new Vector2(X1, Y1);
            Vector2 v2 = new Vector2(X2, Y2);
            GameService.GetService<RenderService>().DrawQuad(
                whiteTileTexture, 
                new Rectangle((int)v1.X, (int)v1.Y, (int)Vector2.Distance(v1, v2), 1), 
                new Rectangle(0, 0, 8, 8), 
                Tint, 
                Vector2.Zero, 
                GetAngleBetween(v1, v2)
            );
        }

        public static void DrawLineBetweenVector3s(Vector3 V1, Vector3 V2, Color Tint)
        {
            Vector2 v1Projection = ProjectVector3(V1);
            Vector2 v2Projection = ProjectVector3(V2);
            DrawLine((int)v1Projection.X, (int)v1Projection.Y, (int)v2Projection.X, (int)v2Projection.Y, Tint);
        }

        public static void DrawCenteredSquareInWorldSpace(Vector3 Position, int W, int H, Color Tint)
        {
            Vector2 screenPosition = ProjectVector3(Position);
            DrawSquare((int)screenPosition.X - W / 2, (int)screenPosition.Y - H / 2, W, H, Tint);
        }

        public static Vector2 ProjectVector3(Vector3 V)
        {
            return new Vector2(V.X, V.Z - V.Y);
        }

        public static float GetAngleBetween(Vector2 V1, Vector2 V2)
        {
            return (float)Math.Atan2(V2.Y - V1.Y, V2.X - V1.X);
        }

        public static Vector2 GetScreenRelativeLocation(Vector3 WorldCoordinate)
        {
            Vector2 projection = ProjectVector3(WorldCoordinate);
            Camera camera = GameService.GetService<SceneService>().CurrentScene.Camera;
            Vector2 result = new Vector2((projection.X - camera.Left) / GameService.GetService<RenderService>().InternalResolutionX, (projection.Y - camera.Top) / GameService.GetService<RenderService>().InternalResolutionY);
            return result;
        }

        public static Box GetBoundaryBox<T>(List<T> Boxes) where T : IDraw
        {
            if (Boxes.Count == 0)
            {
                return new Box();
            }
            Box b = Boxes[0].GetBackingBox().B;
            float minX = b.Left;
            float maxX = b.Right;
            float minY = b.Bottom;
            float maxY = b.Top;
            float minZ = b.Back;
            float maxZ = b.Front;
            foreach (IDraw box in Boxes)
            {
                b = box.GetBackingBox().B;
                minX = Math.Min(b.Left, minX);
                minY = Math.Min(b.Bottom, minY);
                minZ = Math.Min(b.Back, minZ);
                maxX = Math.Max(b.Right, maxX);
                maxY = Math.Max(b.Top, maxY);
                maxZ = Math.Max(b.Front, maxZ);
            }
            return new Box(minX, minY, minZ, maxX - minX, maxY - minY, maxZ - minZ);
        }

        public static Box GetBoundaryBox(Box B1, Box B2)
        {
            float left = Math.Min(B1.Left, B2.Left);
            float bottom = Math.Min(B1.Bottom, B2.Bottom);
            float back = Math.Min(B1.Back, B2.Back);
            return new Box(
                left,
                bottom,
                back,
                Math.Max(B1.Right, B2.Right) - left,
                Math.Max(B1.Top, B2.Top) - bottom,
                Math.Max(B1.Front, B2.Front) - back
            );
        }
    }
}
