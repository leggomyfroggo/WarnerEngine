using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Entities;
using WarnerEngine.Lib.Helpers;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Components.Physics
{
    [Serializable]
    public class BaseWorldTile : IWorldSerializable, ISceneEntity, IDraw
    {
        public enum ShadowReceiverMode { Exact, PointCheck, None }
        private enum VisibilityState { Unknown, Visible, Invisible }

        private static readonly Color CHECK_COLOR_TOP_EVEN = new Color(40, 120, 120);
        private static readonly Color CHECK_COLOR_TOP_ODD = new Color(50, 149, 149);
        private static readonly Color CHECK_COLOR_FRONT_EVEN = new Color(81, 81, 98);
        private static readonly Color CHECK_COLOR_FRONT_ODD = new Color(98, 98, 117);

        [XmlIgnore]
        public BackingBox BackingBox;

        [XmlElement("X")]
        public int x;
        [XmlElement("Y")]
        public int y;
        [XmlElement("Z")]
        public int z;

        [XmlElement("W")]
        public int w;
        [XmlElement("H")]
        public int h;
        [XmlElement("D")]
        public int d;

        [XmlElement("STW")]
        public int sourceTileWidth;
        [XmlElement("STH")]
        public int sourceTileHeight;
        [XmlElement("STD")]
        public int sourceTileDepth;

        [XmlElement("TK")]
        public string textureKey;

        // EXPERIMENTAL DECALS
        [XmlArray("TopFaceTiles")]
        public WorldTileFace[] topFaceTiles;
        [XmlArray("FrontFaceTiles")]
        public WorldTileFace[] frontFaceTiles;

        private int WorldX => x;
        private int WorldY => z - y - h * sourceTileHeight;

        private int WorldTileWidth => sourceTileWidth;
        private int WorldTileHeight => sourceTileHeight + sourceTileDepth;

        [NonSerialized]
        private Texture2D texture;

        [XmlElement("SRC")]
        public ShadowReceiverMode shadowReceiverMode;

        [XmlElement("IT")]
        public BackingBox.IType interactionType;

        [NonSerialized]
        private VisibilityState visibility;
        private VisibilityState Visibility
        {
            get
            {
                if (visibility == VisibilityState.Unknown)
                {
                    visibility = VisibilityState.Invisible;
                    foreach (WorldTileFace tile in topFaceTiles)
                    {
                        if (tile.TextureIndex != -1)
                        {
                            visibility = VisibilityState.Visible;
                            break;
                        }
                    }
                    if (visibility == VisibilityState.Invisible)
                    {
                        foreach (WorldTileFace tile in frontFaceTiles)
                        {
                            if (tile.TextureIndex != -1)
                            {
                                visibility = VisibilityState.Visible;
                                break;
                            }
                        }
                    }
                }
                return visibility;
            }
        }

        public BaseWorldTile()
        {
            shadowReceiverMode = ShadowReceiverMode.Exact;
            interactionType = BackingBox.IType.Static;
            //topTiles = DataStructureHelper.CreateFilledArray(w * d, -1);
            //frontTiles = DataStructureHelper.CreateFilledArray(w * h, -1);
            topFaceTiles = DataStructureHelper.CreateFilledArray(w * d, WorldTileFace.Empty);
            frontFaceTiles = DataStructureHelper.CreateFilledArray(w * d, WorldTileFace.Empty);
            PostSerialize();
        }

        public BaseWorldTile(string TextureKey, int X, int Y, int Z, int W, int H, int D, int SourceTileWidth, int SourceTileHeight, int SourceTileDepth, BackingBox.IType InteractionType = BackingBox.IType.Static)
        {
            textureKey = TextureKey;
            x = X;
            y = Y;
            z = Z;
            w = W;
            h = H;
            d = D;
            sourceTileWidth = SourceTileWidth;
            sourceTileHeight = SourceTileHeight;
            sourceTileDepth = SourceTileDepth;
            shadowReceiverMode = ShadowReceiverMode.Exact;
            interactionType = InteractionType;
            //topTiles = DataStructureHelper.CreateFilledArray(w * d, -1);
            //frontTiles = DataStructureHelper.CreateFilledArray(w * h, -1);
            topFaceTiles = DataStructureHelper.CreateFilledArray(w * d, WorldTileFace.Empty);
            frontFaceTiles = DataStructureHelper.CreateFilledArray(w * d, WorldTileFace.Empty);
            PostSerialize();
        }

        public void OnAdd(Scene ParentScene)
        {
            ParentScene.GetLocalValue<World>(World.WORLD_KEY).AddBox(BackingBox);
        }

        public void OnRemove(Scene ParentScene)
        {
            ParentScene.GetLocalValue<World>(World.WORLD_KEY).RemoveBox(BackingBox);
        }

        public void Draw()
        {
            Vector2 pos = BackingBox.GetFullProjectedPosition2();
            Camera camera = GameService.GetService<SceneService>().CurrentScene.Camera;
            Color shadowTint = shadowReceiverMode == ShadowReceiverMode.Exact ? Color.White : ShadowCasterHelper.GetShadowTintForBox(BackingBox.B);
            Color appliedTint = shadowTint;
            if (topFaceTiles.Length > 0 || frontFaceTiles.Length > 0)
            {
                Rectangle tilePlacer = new Rectangle();
                tilePlacer.Width = sourceTileWidth;
                int minXIndex = camera.Left > BackingBox.Left ? (int)((camera.Left - BackingBox.Left) / sourceTileWidth) : 0;
                int maxXIndex = camera.Right < BackingBox.Right ? (int)Math.Ceiling((camera.Right - BackingBox.Left) / sourceTileWidth) : w;
                for (int sx = minXIndex; sx < maxXIndex; sx++)
                {
                    tilePlacer.X = x + sx * sourceTileWidth;
                    if (sourceTileDepth > 0 && topFaceTiles.Length > 0)
                    {
                        int minZIndex = camera.Top > pos.Y ? (int)((camera.Top - pos.Y) / sourceTileDepth) : 0;
                        int maxZIndex = camera.Bottom < pos.Y + BackingBox.Depth ? (int)Math.Ceiling((camera.Bottom - pos.Y) / sourceTileDepth) : d;
                        tilePlacer.Height = sourceTileDepth;
                        for (int sz = minZIndex; sz < maxZIndex; sz++)
                        {
                            tilePlacer.Y = z - y - h * sourceTileHeight + sz * sourceTileDepth;
                            Texture2D tex = texture;
                            WorldTileFace tileFace = topFaceTiles[sx + sz * w];
                            bool isTiling = false;
                            int t = tileFace.TextureIndex;
                            if (t == -1)
                            {
#if DEBUG
                                isTiling = true;
                                tex = GameService.GetService<IContentService>().GetxAsset<Texture2D>("blank_texture");
                                appliedTint = (sx + sz) % 2 == 0 ? CHECK_COLOR_TOP_EVEN : CHECK_COLOR_TOP_ODD;
#else
                                continue;
#endif
                            }
                            int tx = t % 96;
                            int ty = t / 96;
                            GameService.GetService<RenderService>().DrawQuad(
                                tex,
                                tilePlacer,
                                new Rectangle(tx * 8, ty * 8, sourceTileWidth, sourceTileDepth),
                                appliedTint,
                                IsTiling: isTiling
                            );
                            int decalIndex = tileFace.DecalTextureIndex;
                            if (tileFace.DecalTextureIndex != -1)
                            {
                                int dx = decalIndex % 96;
                                int dy = decalIndex / 96;
                                Vector2 behaviorOffset = tileFace.GetBehavioralOffset(sz + sx);
                                GameService.GetService<RenderService>().DrawQuad(
                                    tex,
                                    new Vector2(tilePlacer.X, tilePlacer.Y) + behaviorOffset,
                                    new Rectangle(dx * 8, dy * 8, sourceTileWidth, sourceTileDepth),
                                    appliedTint
                                );
                                float time = GameService.GetService<StateService>().GetGlobalGameTime();
                            }
                            GameService.GetService<RenderService>().PushAlphaFragment(
                                RenderService.WATER_STACK,
                                GameService.GetService<IContentService>().GetxAsset<Texture2D>("forest_tiles_stencil"),
                                tilePlacer,
                                new Rectangle(tx * 8, ty * 8, sourceTileWidth, sourceTileDepth)
                            );
                            appliedTint = shadowTint;
                            tilePlacer.Y += sourceTileDepth;
                        }
                    }
                    if (sourceTileHeight > 0 && frontFaceTiles.Length > 0)
                    {
                        int minYIndex = camera.Top > pos.Y + BackingBox.Depth ? (int)((camera.Top - (pos.Y + BackingBox.Depth)) / sourceTileHeight) : 0;
                        int maxYIndex = camera.Bottom < pos.Y + BackingBox.Depth + BackingBox.Height ? (int)Math.Ceiling((camera.Bottom - (pos.Y + BackingBox.Depth)) / sourceTileHeight) : h;
                        tilePlacer.Height = sourceTileHeight;
                        for (int sy = minYIndex; sy < maxYIndex; sy++)
                        {
                            tilePlacer.Y = z + sourceTileDepth * d - y + (sy - h) * sourceTileHeight;
                            Texture2D tex = texture;
                            WorldTileFace tileFace = frontFaceTiles[sx + sy * w];
                            bool isTiling = false;
                            int t = tileFace.TextureIndex;
                            if (t == -1)
                            {
#if DEBUG
                                isTiling = true;
                                tex = GameService.GetService<IContentService>().GetxAsset<Texture2D>("blank_texture");
                                appliedTint = (sx + sy) % 2 == 0 ? CHECK_COLOR_FRONT_EVEN : CHECK_COLOR_FRONT_ODD;
#else
                                continue;
#endif
                            }
                            int tx = t % 96;
                            int ty = t / 96;
                            GameService.GetService<RenderService>().DrawQuad(
                                tex,
                                tilePlacer,
                                new Rectangle(tx * 8, ty * 8, sourceTileWidth, sourceTileHeight),
                                appliedTint,
                                IsTiling: isTiling
                            );
                            int decalIndex = tileFace.DecalTextureIndex;
                            if (tileFace.DecalTextureIndex != -1)
                            {
                                int dx = decalIndex % 96;
                                int dy = decalIndex / 96;
                                Vector2 behaviorOffset = tileFace.GetBehavioralOffset(sx + sy);
                                GameService.GetService<RenderService>().DrawQuad(
                                    tex,
                                    new Vector2(tilePlacer.X, tilePlacer.Y) + behaviorOffset,
                                    new Rectangle(dx * 8, dy * 8, sourceTileWidth, sourceTileDepth),
                                    appliedTint
                                );
                                float time = GameService.GetService<StateService>().GetGlobalGameTime();
                            }
                            GameService.GetService<RenderService>().PushAlphaFragment(
                                RenderService.WATER_STACK,
                                GameService.GetService<IContentService>().GetxAsset<Texture2D>("forest_tiles_stencil"),
                                tilePlacer,
                                new Rectangle(tx * 8, ty * 8, sourceTileWidth, sourceTileHeight)
                            );
                            appliedTint = shadowTint;
                            tilePlacer.Y += sourceTileHeight;
                        }
                    }
                    tilePlacer.X += sourceTileWidth;
                }
            }
            DrawShadows();
        }

        public BackingBox GetBackingBox()
        {
            return BackingBox;
        }

        public bool IsSelectable()
        {
            return true;
        }

        public Box GetBoundingBox()
        {
            return BackingBox.B;
        }

        public bool IsVisible()
        {
            return Visibility == VisibilityState.Visible || GameService.GetService<StateService>().IsFlagSet(Flags.LE_DEBUG_RENDER);
        }

        private void DrawShadows()
        {
            if (shadowReceiverMode != ShadowReceiverMode.Exact)
            {
                return;
            }

            List<IShadowCaster> shadowCasters = ShadowCasterHelper.VisibleShadowCasters;
            foreach (IShadowCaster shadowCaster in shadowCasters)
            {
                Box shadowVolume = shadowCaster.GetShadowVolume();
                Box? shadowBox = shadowVolume.GetIntersectionVolume(BackingBox.B);
                if (shadowBox == null || (!BackingBox.B.isRamp && shadowBox.Value.Top < BackingBox.Top))
                {
                    continue;
                }
                Texture2D shadowTexture = shadowCaster.GetShadowTexture();
                int xIndex;
                int yIndex;
                int sourceWidth;
                int sourceHeight;
                if (shadowCaster.ShouldTile())
                {
                    xIndex = (int)(shadowBox.Value.Left - shadowVolume.Left);
                    yIndex = (int)(shadowBox.Value.Back - shadowVolume.Back);
                    sourceWidth = (int)shadowBox?.Width;
                    sourceHeight = (int)shadowBox?.Depth;
                }
                else
                {
                    xIndex = (int)Math.Round((shadowBox.Value.Left - shadowVolume.Left) / shadowVolume.Width * shadowTexture.Width);
                    yIndex = (int)Math.Round((shadowBox.Value.Back - shadowVolume.Back) / shadowVolume.Depth * shadowTexture.Height);
                    sourceWidth = (int)Math.Round(shadowBox.Value.Width / shadowVolume.Width * shadowTexture.Width);
                    sourceHeight = (int)Math.Round(shadowBox.Value.Depth / shadowVolume.Depth * shadowTexture.Height);
                }
                float opacity = (1f - (shadowVolume.Top - BackingBox.B.Top) / shadowVolume.Height) * shadowCaster.GetShadowOpacity();
                if (!BackingBox.B.isRamp)
                {
                    GameService.GetService<RenderService>().PushAlphaFragment(
                        RenderService.SHADOW_STACK,
                        shadowTexture,
                        new Rectangle((int)Math.Round(shadowBox.Value.Left), (int)Math.Round(shadowBox.Value.Back - shadowBox.Value.Top), (int)Math.Round(shadowBox.Value.Width), (int)Math.Round(shadowBox.Value.Depth)),
                        new Rectangle(xIndex, yIndex, sourceWidth, sourceHeight),
                        IsTiling: shadowCaster.ShouldTile(), 
                        Tint: Color.White * opacity
                    );
                    if (shadowBox.Value.Front == BackingBox.B.Front)
                    {
                        GameService.GetService<RenderService>().PushAlphaFragment(
                            RenderService.SHADOW_STACK,
                            shadowTexture,
                            new Rectangle((int)Math.Round(shadowBox.Value.Left), (int)Math.Round(shadowBox.Value.Front - shadowBox.Value.Top), (int)Math.Round(shadowBox.Value.Width), (int)Math.Round(shadowBox.Value.Height)),
                            new Rectangle(xIndex, yIndex + sourceHeight - 1, sourceWidth, 1),
                            IsTiling: shadowCaster.ShouldTile(), 
                            Tint: Color.White * opacity
                        );
                    }
                }
                else
                {
                    GameService.GetService<RenderService>().PushAlphaFragment(
                        RenderService.SHADOW_STACK,
                        shadowTexture,
                        new Rectangle((int)Math.Round(shadowBox.Value.Left), ((int)shadowBox.Value.Back - (int)BackingBox.Back) / 2 + (int)Math.Round(shadowBox.Value.Back - BackingBox.B.Top), (int)Math.Round(shadowBox.Value.Width), (int)Math.Round(shadowBox.Value.Depth * 1.5f)),
                        new Rectangle(xIndex, yIndex, sourceWidth, sourceHeight),
                        IsTiling: shadowCaster.ShouldTile(), 
                        Tint: Color.White * opacity
                    );
                }
            }
        }

        public virtual void PostSerialize()
        {
            if (textureKey != null)
            {
                texture = GameService.GetService<IContentService>().GetxAsset<Texture2D>(textureKey);
            }
            BackingBox = new BackingBox(interactionType, x, y, z, w * sourceTileWidth, h * sourceTileHeight, d * sourceTileDepth, SortMode: BackingBox.SortingMode.Box);
        }
    }
}
