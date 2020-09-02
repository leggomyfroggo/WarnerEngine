using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ProjectWarnerShared.Lib.Entities;

namespace ProjectWarnerShared.Lib.Structure
{
    public struct ScreenSpacePartition<T> where T : IDraw
    {
        private const int PARTITION_WIDTH = 32;
        private const int PARTITION_HEIGHT = 32;

        private int width;
        private int height;
        private List<T>[,] partitions;

        public ScreenSpacePartition(float CameraLeft, float CameraTop, int ScreenWidth, int ScreenHeight, IEnumerable<T> Entities)
        {
            width = ScreenWidth / PARTITION_WIDTH;
            height = ScreenHeight / PARTITION_HEIGHT;
            partitions = new List<T>[width, height];
            foreach (T entity in Entities)
            {
                Vector2 screenPosition = entity.GetBackingBox().GetFullProjectedPosition2() - new Vector2(CameraLeft, CameraTop);
                int pX = (int)Math.Floor(screenPosition.X / PARTITION_WIDTH);
                int pY = (int)Math.Floor(screenPosition.Y / PARTITION_HEIGHT);
                int w = (int)Math.Ceiling(entity.GetBackingBox().Width / PARTITION_WIDTH);
                int h = (int)Math.Ceiling(entity.GetBackingBox().ProjectedHeight / PARTITION_HEIGHT);
                for (int x = Math.Max(pX, 0); x < Math.Min(pX + w + 1, width); x++)
                {
                    for (int y = Math.Max(pY, 0); y < Math.Min(pY + h + 1, height); y++)
                    {
                        if (partitions[x, y] == null)
                        {
                            partitions[x, y] = new List<T>();
                        }
                        partitions[x, y].Add(entity);
                    }
                }
            }
        }

        public HashSet<T> GetEntitiesNearEntity(float CameraLeft, float CameraTop, T Entity)
        {
            HashSet<T> entities = new HashSet<T>();
            Vector2 screenPosition = Entity.GetBackingBox().GetFullProjectedPosition2() - new Vector2(CameraLeft, CameraTop);
            int pX = (int)Math.Floor(screenPosition.X / PARTITION_WIDTH);
            int pY = (int)Math.Floor(screenPosition.Y / PARTITION_HEIGHT);
            int w = (int)Math.Ceiling(Entity.GetBackingBox().Width / PARTITION_WIDTH);
            int h = (int)Math.Ceiling(Entity.GetBackingBox().ProjectedHeight / PARTITION_HEIGHT);
            for (int x = Math.Max(pX, 0); x < Math.Min(pX + w + 1, width); x++)
            {
                for (int y = Math.Max(pY, 0); y < Math.Min(pY + h + 1, height); y++)
                {
                    if (partitions[x, y] == null)
                    {
                        continue;
                    }
                    foreach (T entity in partitions[x, y])
                    {
                        entities.Add(entity);
                    }
                }
            }
            return entities;
        }
    }
}
