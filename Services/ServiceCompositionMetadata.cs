using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectWarnerShared.Services
{
    public struct ServiceCompositionMetadata : IEquatable<ServiceCompositionMetadata>
    {
        public static readonly ServiceCompositionMetadata Empty = new ServiceCompositionMetadata()
        {
            RenderTargetKey = null,
            Position = Vector2.Zero,
            Priority = 0,
            Tint = Color.White,
            CompositeEffect = null,
        };

        public static bool operator ==(ServiceCompositionMetadata a, ServiceCompositionMetadata b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ServiceCompositionMetadata a, ServiceCompositionMetadata b)
        {
            return !a.Equals(b);
        }

        public string RenderTargetKey;
        public Vector2 Position;
        public int Priority;
        public Color Tint;
        public Effect CompositeEffect;

        public bool Equals(ServiceCompositionMetadata other)
        {
            return RenderTargetKey == other.RenderTargetKey &&
                   Position.Equals(other.Position) &&
                   Priority == other.Priority &&
                   Tint.Equals(other.Tint) &&
                   CompositeEffect == other.CompositeEffect;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ServiceCompositionMetadata))
            {
                return false;
            }

            var metadata = (ServiceCompositionMetadata)obj;
            return RenderTargetKey == metadata.RenderTargetKey &&
                   Position.Equals(metadata.Position) &&
                   Priority == metadata.Priority &&
                   Tint.Equals(metadata.Tint) &&
                   CompositeEffect == metadata.CompositeEffect;
        }

        public override int GetHashCode()
        {
            var hashCode = 493515870;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RenderTargetKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Position);
            hashCode = hashCode * -1521134295 + Priority.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(Tint);
            hashCode = hashCode * -1521134295 + EqualityComparer<Effect>.Default.GetHashCode(CompositeEffect);
            return hashCode;
        }
    }
}
