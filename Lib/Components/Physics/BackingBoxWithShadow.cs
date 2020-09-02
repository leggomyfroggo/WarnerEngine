using Microsoft.Xna.Framework;

using ProjectWarnerShared.Entities;
using ProjectWarnerShared.Lib.Components;

namespace ProjectWarnerShared.Lib
{
    public sealed class BackingBoxWithShadow : BackingBox
    {
        public Box ShadowVolume { get; private set; }

        private float xOffset;
        private float zOffset;
        private float wExpansion;
        private float dExpansion;
        private float shadowDepth;

        public BackingBoxWithShadow(IType InteractionType, float X, float Y, float Z, float Width, float Height, float Depth, bool IsTriangle = false, bool IsRamp = false, float ShadowDepth = ShadowCasterHelper.GLOBAL_SHADOW_DEPTH) : base(InteractionType, X, Y, Z, Width, Height, Depth, IsTriangle = false, IsRamp = false)
        {
            UpdateShadowVolume();
            shadowDepth = ShadowDepth;
        }

        public BackingBoxWithShadow(IType InteractionType, float X, float Y, float Z, float Width, float Height, float Depth, float XOffset, float ZOffset, float WExpansion, float DExpansion, bool IsTriangle = false, bool IsRamp = false, float ShadowDepth = ShadowCasterHelper.GLOBAL_SHADOW_DEPTH) : base(InteractionType, X, Y, Z, Width, Height, Depth, IsTriangle = false, IsRamp = false)
        {
            shadowDepth = ShadowDepth;
            xOffset = XOffset;
            zOffset = ZOffset;
            wExpansion = WExpansion;
            dExpansion = DExpansion;
            shadowDepth = ShadowDepth;
            UpdateShadowVolume();
        }

        public BackingBoxWithShadow(IType InteractionType, Box B, float ShadowDepth = ShadowCasterHelper.GLOBAL_SHADOW_DEPTH) : base(InteractionType, B)
        {
            UpdateShadowVolume();
            shadowDepth = ShadowDepth;
        }

        public override void Teleport(Vector3 Position)
        {
            base.Teleport(Position);
            UpdateShadowVolume();
        }

        private void UpdateShadowVolume()
        {
            ShadowVolume = new Box(Left + xOffset - wExpansion / 2, Bottom, Back + zOffset - dExpansion / 2, Width + wExpansion, -shadowDepth, Depth + dExpansion);
        }
    }
}
