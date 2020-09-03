using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.Content
{
    [XmlRoot("Animation")]
    public class Animation
    {
        [XmlElement("AnimationKey")]
        public string animationKey;

        [XmlElement("SpriteSheetKey")]
        public string spriteSheetKey;

        [XmlElement("TileWidth")]
        public int tileWidth;
        [XmlElement("TileHeight")]
        public int tileHeight;

        [XmlElement("FirstXIndex")]
        public int firstXIndex;
        [XmlElement("FirstYIndex")]
        public int firstYIndex;
        [XmlElement("LastXIndex")]
        public int lastXIndex;
        [XmlElement("LastYIndex")]
        public int lastYIndex;

        [XmlArray("PerFrameOrigins")]
        public Vector2[] perFrameOrigins;

        private Dictionary<string, Vector3[]> hotspots;
        [XmlIgnore]
        public Dictionary<string, Vector3[]> Hotspots
        {
            get
            {
                return hotspots;
            }
            set
            {
                hotspots = value;
                var list = new List<(string, Vector3[])>();
                foreach (string key in hotspots.Keys)
                {
                    list.Add((key, hotspots[key]));
                }
                hotspotList = list.ToArray();
            }
        }

        private (string, Vector3[])[] hotspotList;
        [XmlArray("Hotspots")]
        public (string, Vector3[])[] HotspotList
        {
            get
            {
                return hotspotList;
            }
            set
            {
                hotspotList = value;
                hotspots = new Dictionary<string, Vector3[]>();
                foreach ((string key, Vector3[] points) in hotspotList)
                {
                    hotspots[key] = points;
                }
            }
        }


        [XmlElement("ShouldLoop")]
        public bool shouldLoop;

        [XmlArray("FrameSpeeds")]
        public int[] frameSpeeds;

        public int XFrameCount => (lastXIndex - firstXIndex + 1);
        public int YFrameCount => (lastYIndex - firstYIndex + 1);
        public int FrameCount => XFrameCount * YFrameCount;

        public Animation()
        {
            animationKey = "";
            tileWidth = 0;
            tileHeight = 0;
            firstXIndex = 0;
            firstYIndex = 0;
            lastXIndex = 0;
            lastYIndex = 0;
            frameSpeeds = new int[] { };
            hotspots = new Dictionary<string, Vector3[]>();
            shouldLoop = false;
        }

        public Animation SetAnimationKey(string AnimationKey)
        {
            animationKey = AnimationKey;
            return this;
        }

        public Animation SetSpriteSheetKey(string SpriteSheetKey)
        {
            spriteSheetKey = SpriteSheetKey;
            return this;
        }

        public Animation SetTileWidth(int TileWidth)
        {
            tileWidth = TileWidth;
            return this;
        }

        public Animation SetTileHeight(int TileHeight)
        {
            tileHeight = TileHeight;
            return this;
        }

        public Animation SetFirstXIndex(int FirstXIndex)
        {
            firstXIndex = FirstXIndex;
            return this;
        }

        public Animation SetFirstYIndex(int FirstYIndex)
        {
            firstYIndex = FirstYIndex;
            return this;
        }

        public Animation SetLastXIndex(int LastXIndex)
        {
            lastXIndex = LastXIndex;
            return this;
        }

        public Animation SetLastYIndex(int LastYIndex)
        {
            lastYIndex = LastYIndex;
            return this;
        }

        public Animation SetPerFrameOrigins(Vector2[] PerFrameOrigins)
        {
            perFrameOrigins = PerFrameOrigins;
            return this;
        }

        public Animation SetHotspots(Dictionary<string, Vector3[]> PHotSpots)
        {
            Hotspots = PHotSpots;
            return this;
        }

        public Animation SetFrameSpeeds(int[] FrameSpeeds)
        {
            frameSpeeds = FrameSpeeds;
            return this;
        }

        public Animation SetShouldLoop()
        {
            shouldLoop = true;
            return this;
        }

        public void Draw(RenderService RM, Vector2 Position, int Frame, Color? Tint, float Scale = 1f, float Depth = 1f, float Angle = 0f, string TargetAlphaStack = null, bool DuplicateToAlphaStack = false)
        {
            Frame = Frame % FrameCount;
            int frameXIndex = Frame % XFrameCount + firstXIndex;
            int frameYIndex = Frame / XFrameCount + firstYIndex;
            int frameX = frameXIndex * tileWidth;
            int frameY = frameYIndex * tileHeight;
            Rectangle destRect = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y), tileWidth, tileHeight);
            Rectangle sourceRect = new Rectangle(frameX, frameY, tileWidth, tileHeight);
            Vector2 origin = GetOriginAtFrame(Frame);
            if (DuplicateToAlphaStack || TargetAlphaStack == null)
            {
                RM.DrawQuad(
                    GameService.GetService<IContentService>().GetxAsset<Texture2D>(spriteSheetKey),
                    destRect,
                    sourceRect,
                    Tint,
                    origin,
                    Angle
                );
            }
            
            if (TargetAlphaStack != null)
            {
                destRect.X -= (int)origin.X;
                destRect.Y -= (int)origin.Y;
                RM.PushAlphaFragment(TargetAlphaStack, GameService.GetService<IContentService>().GetxAsset<Texture2D>(spriteSheetKey), destRect, sourceRect);
            }
        }

        public int GetFrameSpeed(int Frame) => frameSpeeds[Frame];
        public float GetFrameLength(int Frame) => GetFrameSpeed(Frame) / 1000f;

        public Vector2 GetOriginAtFrame(int Frame)
        {
            if (perFrameOrigins == null)
            {
                return Vector2.Zero;
            }
            return perFrameOrigins[Frame % perFrameOrigins.Length];
        }

        public Vector3 GetHotspotAtFrame(string Hotspot, int Frame) => hotspots.ContainsKey(Hotspot) ? hotspots[Hotspot][Frame % hotspots[Hotspot].Length] : Vector3.Zero;
        public Vector2 GetHotspot2AtFrame(string Hotspot, int Frame) {
            Vector3 hotspot = GetHotspotAtFrame(Hotspot, Frame);
            return new Vector2(hotspot.X, hotspot.Y);
        }
        public float GetHotspotDepthAtFrame(string Hotspot, int Frame) => GetHotspotAtFrame(Hotspot, Frame).Z;
        public (Vector2, float) GetHotspot2AndDepthAtFrame(string Hotspot, int Frame) => (GetHotspot2AtFrame(Hotspot, Frame), GetHotspotDepthAtFrame(Hotspot, Frame));

        public Animation Serialize()
        {
            using (FileStream fs = File.Create("Animations\\" + animationKey + ".xml"))
            {
                XmlSerializer s = new XmlSerializer(typeof(Animation));
                s.Serialize(fs, this);
            }
            return this;
        }
    }
}
