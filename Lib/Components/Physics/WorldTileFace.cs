using System;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Components.Physics
{
    public struct WorldTileFace
    {
        public static readonly WorldTileFace Empty = new WorldTileFace()
        {
            TextureIndex = -1,
            DecalTextureIndex = -1,
            DecalOffsetX = 0,
            DecalOffsetY = 0,
            DBehavior = DecalBehavior.None,
            DecalBehaviorSpeed = 0,
            DecalBehaviorRangeX = 0,
            DecalBehaviorRangeY = 0,
        };

        public enum DecalBehavior { None, HappyBounce, Windy }

        [XmlAttribute("TextureIndex")]
        public int TextureIndex;

        [XmlAttribute("DecalTextureIndex")]
        public int DecalTextureIndex;
        [XmlAttribute("DecalOffsetX")]
        public int DecalOffsetX;
        [XmlAttribute("DecalOffsetY")]
        public int DecalOffsetY;
        [XmlAttribute("DecalBehavior")]
        public DecalBehavior DBehavior;
        [XmlAttribute("DecalBehaviorSpeed")]
        public int DecalBehaviorSpeed;
        [XmlAttribute("DecalBehaviorRangeX")]
        public int DecalBehaviorRangeX;
        [XmlAttribute("DecalBehaviorRangeY")]
        public int DecalBehaviorRangeY;

        public Vector2 GetBehavioralOffset(float TimeOffset = 0f)
        {
            if (DBehavior == DecalBehavior.HappyBounce)
            {
                float time = GameService.GetService<IStateService>().GetGlobalGameTime();
                return new Vector2(
                    (float)Math.Sin(time / DecalBehaviorSpeed) * DecalBehaviorRangeX + DecalOffsetX,
                    (float)Math.Abs(Math.Cos(time / DecalBehaviorSpeed)) * DecalBehaviorRangeY + DecalOffsetY
                );
            }
            else if (DBehavior == DecalBehavior.Windy)
            {
                float timeInSeconds = GameService.GetService<IStateService>().GetGlobalGameTime() / 1000f;
                double innerSinTerm = (timeInSeconds * 4) + TimeOffset;
                return new Vector2(
                    (float)(Math.Sin(innerSinTerm) + Math.Sin(innerSinTerm * 1.5)) * DecalBehaviorRangeX,
                    0f
                );
            }
            return Vector2.Zero;
        }
    }
}
