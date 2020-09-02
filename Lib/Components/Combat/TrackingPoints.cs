using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Helpers;

namespace WarnerEngine.Lib.Components.Combat
{
    public class TrackingPoints
    {
        private BackingBox centralBox;
        private Dictionary<Enums.Direction, BackingBox> availablePoints;

        public TrackingPoints(BackingBox CentralBox)
        {
            centralBox = CentralBox;
            availablePoints = new Dictionary<Enums.Direction, BackingBox>(4);
        }

        public Enums.Direction? TryClaimNearestPoint(BackingBox SeekingBox, float TargetDistance)
        {
            Enums.Direction? nearestDirection = null;
            float minDistance = 0;
            foreach (Enums.Direction dir in Enum.GetValues(typeof(Enums.Direction)))
            {
                if (availablePoints.ContainsKey(dir))
                {
                    if (availablePoints[dir] != SeekingBox)
                    {
                        continue;
                    }
                    else
                    {
                        availablePoints.Remove(dir);
                    }
                }
                Vector3 potentialTrackingPoint = centralBox.GetCenterPoint() + GraphicsHelper.GetVector3FromDirection(dir) * TargetDistance;
                float currentDistance = Vector3.Distance(potentialTrackingPoint, SeekingBox.GetCenterPoint());
                if (nearestDirection == null || currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    nearestDirection = dir;
                }
            }
            if (nearestDirection.HasValue)
            {
                availablePoints[nearestDirection.Value] = SeekingBox;
            }
            return nearestDirection;
        }

        public void RelinquishPoint(BackingBox SeekingBox)
        {
            foreach (Enums.Direction dir in Enum.GetValues(typeof(Enums.Direction)))
            {
                if (availablePoints.ContainsKey(dir))
                {
                    if (availablePoints[dir] == SeekingBox)
                    {
                        availablePoints.Remove(dir);
                        return;
                    }
                }
            }
        }
    }
}
