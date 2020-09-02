using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib
{
    public class SpatialMaximaMap
    {
        private Dictionary<(int, int, int), float> spatialMaxima;
        public int CellWidth { get; private set; }
        public int CellHeight { get; private set; }
        public int CellDepth { get; private set; }

        public SpatialMaximaMap(int CellWidth, int CellHeight, int CellDepth)
        {
            spatialMaxima = new Dictionary<(int, int, int), float>();
            this.CellWidth = CellWidth;
            this.CellHeight = CellHeight;
            this.CellDepth = CellDepth;
        }

        public void AttemptInsert(float Value, float X, float Y, float Z)
        {
            var key = ((int)X / CellWidth, (int)Y / CellHeight, (int)Z / CellDepth);
            if (!spatialMaxima.ContainsKey(key))
            {
                spatialMaxima[key] = Value;
            }
            else
            {
                spatialMaxima[key] = Math.Max(spatialMaxima[key], Value);
            }
        }

        public void AttemptInsert(float Value, Vector3 Point)
        {
            AttemptInsert(Value, Point.X, Point.Y, Point.Z);
        }

        public float? Get(float X, float Y, float Z)
        {
            var key = ((int)X / CellWidth, (int)Y / CellHeight, (int)Z / CellDepth);
            if (!spatialMaxima.ContainsKey(key))
            {
                return null;
            }
            return spatialMaxima[key];
        }

        public float? Get(Vector3 Point)
        {
            return Get(Point.X, Point.Y, Point.Z);
        }

        public void Clear()
        {
            spatialMaxima.Clear();
        }

        public void Attenuate(float Factor, float Threshhold)
        {
            var Keys = spatialMaxima.Keys.ToArray();
            foreach ((int, int, int) key in Keys)
            {
                spatialMaxima[key] *= Factor;
                if (spatialMaxima[key] < Threshhold)
                {
                    spatialMaxima.Remove(key);
                }
            }
        }
    }
}
