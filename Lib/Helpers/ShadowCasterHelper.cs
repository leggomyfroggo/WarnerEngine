using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Helpers
{
    public class ShadowCasterHelper
    {
        public const float GLOBAL_SHADOW_DEPTH = 50f;

        public const float GLOBAL_SHADOW_OPACITY = 0.4f;

        public static readonly SpatialMaximaMap SpatialShadowMap = new SpatialMaximaMap(8, 8, 8);

        public static readonly Color GLOBAL_SHADOW_COLOR = new Color(0, 0, 200);

        public static readonly Vector3 SHADOW_2_SAMPLE_1 = Vector3.Normalize(new Vector3(-1, 0, -1));
        public static readonly Vector3 SHADOW_2_SAMPLE_2 = Vector3.Normalize(new Vector3(1, 0, -1));

        public static readonly Vector3 SHADOW_3_SAMPLE_1 = Vector3.Normalize(new Vector3(-1, 0, 1));
        public static readonly Vector3 SHADOW_3_SAMPLE_2 = Vector3.Normalize(new Vector3(-0.259f, 0, -0.966f));
        public static readonly Vector3 SHADOW_3_SAMPLE_3 = Vector3.Normalize(new Vector3(0.966f, 0, 0.259f));

        private static Dictionary<Texture2D, byte[]> cachedTextureData;
        private static Dictionary<Texture2D, float[]> cachedTextureSumData;

        public static List<IShadowCaster> VisibleShadowCasters { get; private set; }

        static ShadowCasterHelper()
        {
            cachedTextureData = new Dictionary<Texture2D, byte[]>();
            cachedTextureSumData = new Dictionary<Texture2D, float[]>();
            VisibleShadowCasters = new List<IShadowCaster>();
        }

        public static void CacheVisibleShadowCasters()
        {
            Scene currentScene = GameService.GetService<SceneService>().CurrentScene;
            VisibleShadowCasters = currentScene
                .GetEntitiesOfType<IShadowCaster>()
                .Where(caster => currentScene.Camera.ContainsBox(caster.GetShadowVolume()))
                .ToList();
        }

        public static Color GetShadowTintForBox(Box B, IShadowCaster CallingShadowCaster = null)
        {
            List<IShadowCaster> shadowCasters = VisibleShadowCasters;
            float darkestTint = 1f;
            foreach (IShadowCaster shadowCaster in shadowCasters)
            {
                if (shadowCaster == CallingShadowCaster)
                {
                    continue;
                }
                Box shadowVolume = shadowCaster.GetShadowVolume();
                Box? shadowBox = shadowVolume.GetIntersectionVolume(B);
                if (shadowBox == null)
                {
                    continue;
                }
                Texture2D shadowTexture = shadowCaster.GetShadowTexture();
                int x1;
                int y1;
                int x2;
                int y2;
                if (shadowCaster.ShouldTile())
                {
                    x1 = (int)(shadowBox.Value.Left - shadowVolume.Left);
                    y1 = (int)(shadowBox.Value.Back - shadowVolume.Back);
                    x2 = x1 + (int)shadowBox.Value.Width;
                    y2 = y1 + (int)shadowBox.Value.Depth;
                }
                else
                {
                    x1 = (int)Math.Floor((shadowBox.Value.Left - shadowVolume.Left) / shadowVolume.Width * shadowTexture.Width);
                    y1 = (int)Math.Floor((shadowBox.Value.Back - shadowVolume.Back) / shadowVolume.Depth * shadowTexture.Height);
                    x2 = x1 + (int)Math.Floor((shadowBox.Value.Width / shadowVolume.Width) * (shadowTexture.Width - 1));
                    y2 = y1 + (int)Math.Floor((shadowBox.Value.Depth / shadowVolume.Depth) * (shadowTexture.Height - 1));
                }
                float a = GetShadowStrengthOverArea(shadowTexture, x1, y1, x2, y2) * (shadowBox.Value.TopDownSurfaceArea / B.TopDownSurfaceArea) * shadowCaster.GetShadowOpacity();
                if (a > 0)
                {
                    a = 1 - a;
                    if (a < darkestTint)
                    {
                        darkestTint = a;
                    }
                }
            }
            if (darkestTint == 1f)
            {
                return Color.White;
            }
            float invertDarkest = 1 - darkestTint;
            return new Color(
                1 - (invertDarkest - GLOBAL_SHADOW_COLOR.R / 255f * invertDarkest), 
                1 - (invertDarkest - GLOBAL_SHADOW_COLOR.G / 255f * invertDarkest), 
                1 - (invertDarkest - GLOBAL_SHADOW_COLOR.B / 255f * invertDarkest)
            );
        }

        private static float GetShadowFactorForPoint(Vector3 P, IShadowCaster CallingShadowCaster, List<IShadowCaster> PreFilteredShadowCasters)
        {
            float? cachedValue = SpatialShadowMap.Get(P);
            if (cachedValue.HasValue)
            {
                return cachedValue.Value;
            }
            List<IShadowCaster> shadowCasters = PreFilteredShadowCasters == null 
                ? VisibleShadowCasters 
                : PreFilteredShadowCasters;
            byte darkestTint = 0;
            foreach (IShadowCaster shadowCaster in shadowCasters)
            {
                if (shadowCaster == CallingShadowCaster)
                {
                    continue;
                }
                Box shadowVolume = shadowCaster.GetShadowVolume();
                if (shadowVolume.Top < P.Y || !shadowVolume.DoesContainPoint(P))
                {
                    continue;
                }
                Texture2D shadowTexture = shadowCaster.GetShadowTexture();
                byte[] colorData = GetTextureData(shadowTexture);
                int ix;
                int iy;
                if (shadowCaster.ShouldTile())
                {
                    ix = (int)(P.X - shadowVolume.Left) % shadowTexture.Width;
                    iy = (int)(P.Z - shadowVolume.Back) % shadowTexture.Height;
                }
                else
                {
                    ix = (int)Math.Floor((P.X - shadowVolume.Left) / shadowVolume.Width * shadowTexture.Width);
                    iy = (int)Math.Floor((P.Z - shadowVolume.Back) / shadowVolume.Depth * shadowTexture.Height);
                }
                byte pointColorA = colorData[ix + iy * shadowTexture.Width];
                if (pointColorA > darkestTint)
                {
                    darkestTint = pointColorA;
                    if (darkestTint == byte.MaxValue)
                    {
                        break;
                    }
                }
            }
            SpatialShadowMap.AttemptInsert(darkestTint / 255f, P);
            return darkestTint / 255f;
        }

        public static Color GetShadowTintForPoint(Vector3 P, IShadowCaster CallingShadowCaster = null, List<IShadowCaster> PreFilteredShadowCasters = null)
        {
            float shadowFactor = GetShadowFactorForPoint(P, CallingShadowCaster, PreFilteredShadowCasters);
            if (shadowFactor == 0)
            {
                return Color.White;
            }
            float colorChannel = shadowFactor * GLOBAL_SHADOW_OPACITY;
            return new Color(
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.R / 255f * colorChannel),
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.G / 255f * colorChannel),
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.B / 255f * colorChannel)
            );
        }

        public static Color Get2SampledShadowTintForPoint(Vector3 P, float Radius, IShadowCaster CallingShadowCaster = null, List<IShadowCaster> PreFilteredShadowCasters = null)
        {
            float sampledShadowFactor = (
                GetShadowFactorForPoint(P + SHADOW_2_SAMPLE_1 * Radius, CallingShadowCaster, PreFilteredShadowCasters) +
                GetShadowFactorForPoint(P + SHADOW_2_SAMPLE_2 * Radius, CallingShadowCaster, PreFilteredShadowCasters)
            ) / 2f;
            if (sampledShadowFactor == 0)
            {
                return Color.White;
            }
            float colorChannel = sampledShadowFactor * GLOBAL_SHADOW_OPACITY;
            return new Color(
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.R / 255f * colorChannel),
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.G / 255f * colorChannel),
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.B / 255f * colorChannel)
            );
        }

        public static Color Get3SampledShadowTintForPoint(Vector3 P, float Radius, IShadowCaster CallingShadowCaster = null, List<IShadowCaster> PreFilteredShadowCasters = null)
        {
            float sampledShadowFactor = (
                GetShadowFactorForPoint(P + SHADOW_3_SAMPLE_1 * Radius, CallingShadowCaster, PreFilteredShadowCasters) +
                GetShadowFactorForPoint(P + SHADOW_3_SAMPLE_2 * Radius, CallingShadowCaster, PreFilteredShadowCasters) +
                GetShadowFactorForPoint(P + SHADOW_3_SAMPLE_3 * Radius, CallingShadowCaster, PreFilteredShadowCasters)
            ) / 3f;
            if (sampledShadowFactor == 0)
            {
                return Color.White;
            }
            float colorChannel = sampledShadowFactor * GLOBAL_SHADOW_OPACITY;
            return new Color(
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.R / 255f * colorChannel),
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.G / 255f * colorChannel),
                1 - (colorChannel - GLOBAL_SHADOW_COLOR.B / 255f * colorChannel)
            );
        }

        private static byte[] GetTextureData(Texture2D Texture)
        {
            if (!cachedTextureData.ContainsKey(Texture))
            {
                Color[] colorData = new Color[Texture.Width * Texture.Height];
                Texture.GetData(colorData);
                cachedTextureData[Texture] = colorData.Select(c => c.A).ToArray();
            }
            return cachedTextureData[Texture];
        }

        private static float[] GetTextureSumData(Texture2D Texture)
        {
            byte[] shadowData = GetTextureData(Texture);
            if (!cachedTextureSumData.ContainsKey(Texture))
            {
                float[] sumData = new float[Texture.Width * Texture.Height];
                for (int y = 0; y < Texture.Height; y++)
                {
                    float rowSum = 0;
                    for (int x = 0; x < Texture.Width; x++)
                    {
                        int index = x + y * Texture.Width;
                        rowSum += shadowData[index] / 255f;
                        sumData[index] = rowSum;
                        if (y > 0)
                        {
                            sumData[index] += sumData[index - Texture.Width];
                        }
                    }
                }
                cachedTextureSumData[Texture] = sumData;
            }
            return cachedTextureSumData[Texture];
        }

        private static float GetShadowStrengthOverArea(Texture2D Texture, int X1, int Y1, int X2, int Y2)
        {
            float[] sumData = GetTextureSumData(Texture);
            float finalSum = sumData[(X2 % Texture.Width) + (Y2 % Texture.Height) * Texture.Width];
            bool removedLeft = false;
            bool shouldRemoveCorner = false;
            if (X1 > 0)
            {
                finalSum -= sumData[((X1 - 1) % Texture.Width) + (Y2 % Texture.Height) * Texture.Width];
                removedLeft = true;
            }
            if (Y1 > 0)
            {
                finalSum -= sumData[(X2 % Texture.Width) + ((Y1 - 1) % Texture.Height) * Texture.Width];
                if (removedLeft)
                {
                    shouldRemoveCorner = true;
                }
            }
            if (shouldRemoveCorner)
            {
                finalSum += sumData[((X1 - 1) % Texture.Width) + ((Y1 - 1) % Texture.Height) * Texture.Width];
            }
            return finalSum / ((X2 - X1 + 1) * (Y2 - Y1 + 1));
        }
    }
}
