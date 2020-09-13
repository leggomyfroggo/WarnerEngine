using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WarnerEngine.Lib.Components.Animation;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Components
{
    public class AnimationPlayer : IAnimationPlayer
    {
        private static Dictionary<(string, Enums.Direction), string> cachedNormalizedAnimationNames;

        public BackingBox BackingPositionable { get; set; }

        private string currentAnimationName;
        public string CurrentAnimationName
        {
            get
            {
                return currentAnimationName;
            }
            protected set
            {
                normalizedAnimationName = null;
                currentAnimationName = value;
            }
        }

        private string normalizedAnimationName;
        protected string NormalizedAnimationName
        {
            get
            {
                if (normalizedAnimationName == null)
                {
                    if (!cachedNormalizedAnimationNames.ContainsKey((CurrentAnimationName, Direction)))
                    {
                        cachedNormalizedAnimationNames[(CurrentAnimationName, Direction)] = CurrentAnimationName + "_" + Enums.DirectionNameFromEnum(Direction);
                    }
                    normalizedAnimationName = cachedNormalizedAnimationNames[(CurrentAnimationName, Direction)];
                }
                return normalizedAnimationName;
            }
            set
            {
                normalizedAnimationName = value;
            }
        }

        private Enums.Direction direction;
        public Enums.Direction Direction
        {
            get
            {
                return direction;
            }
            protected set
            {
                normalizedAnimationName = null;
                direction = value;
            }
        }

        public float SpeedMultiplier { get; set; }

        public bool DidChangeFrame { get; protected set; }

        public int CurrentFrame { get; protected set; }

        public float FrameTime { get; protected set; }

        public bool IsPlaying { get; protected set; }

        public Dictionary<string, List<(string, string)>> AnimToSubAnim { get; protected set; }

        static AnimationPlayer()
        {
            cachedNormalizedAnimationNames = new Dictionary<(string, Enums.Direction), string>();
        }

        public AnimationPlayer(BackingBox BackingPositionable) : this()
        {
            this.BackingPositionable = BackingPositionable;
        }

        public AnimationPlayer()
        {
            CurrentAnimationName = null;
            NormalizedAnimationName = null;
            Direction = Enums.Direction.down;
            CurrentFrame = 0;
            FrameTime = 0;
            IsPlaying = false;
            AnimToSubAnim = new Dictionary<string, List<(string, string)>>();
            SpeedMultiplier = 1f;
        }

        public IAnimationPlayer SetAnimation(string Name, bool ResetState = true)
        {
            if (CurrentAnimationName == Name)
            {
                return this;
            }

            CurrentAnimationName = Name;
            if (ResetState)
            {
                ResetFrameState();
                IsPlaying = false;
            }

            return this;
        }

        public IAnimationPlayer SetDirection(Enums.Direction Direction)
        {
            this.Direction = Direction;
            return this;
        }

        public IAnimationPlayer Play()
        {
            IsPlaying = true;
            return this;
        }

        public IAnimationPlayer Pause()
        {
            IsPlaying = false;
            return this;
        }

        public IAnimationPlayer PlayFromBeginning()
        {
            IsPlaying = true;
            ResetFrameState();
            return this;
        }

        public IAnimationPlayer Stop()
        {
            IsPlaying = false;
            ResetFrameState();
            return this;
        }


        public IAnimationPlayer SetAnimToSubAnim(string AnimName, string SubAnimName, string AnimHotspot)
        {
            if (!AnimToSubAnim.ContainsKey(AnimName))
            {
                AnimToSubAnim[AnimName] = new List<(string, string)>();
            }
            AnimToSubAnim[AnimName].Add((SubAnimName, AnimHotspot));
            return this;
        }

        public float GetProgress()
        {
            return (float)CurrentFrame / GetAsset<Content.Animation>().FrameCount;
        }

        protected void ResetFrameState()
        {
            CurrentFrame = 0;
            FrameTime = 0f;
        }

        public void PreDraw(float DT)
        {
            DidChangeFrame = false;
            if (IsPlaying)
            {
                Content.Animation currentAnimation = GameService.GetService<IContentService>().GetAsset<Content.Animation>(NormalizedAnimationName);
                FrameTime += DT * SpeedMultiplier;
                while (FrameTime >= currentAnimation.GetFrameLength(CurrentFrame))
                {
                    DidChangeFrame = true;
                    FrameTime -= currentAnimation.GetFrameLength(CurrentFrame);
                    CurrentFrame += 1;
                    if (CurrentFrame == currentAnimation.FrameCount)
                    {
                        if (currentAnimation.shouldLoop)
                        {
                            CurrentFrame = 0;
                            continue;
                        }
                        IsPlaying = false;
                        FrameTime = 0f;
                        CurrentFrame = currentAnimation.FrameCount - 1;
                        break;
                    }
                }
            }
        }

        public void Draw(Color? Tint = null, float Angle = 0f, string TargetAlphaStack = null, bool DuplicateToAlphaStack = false)
        {
            Vector2 position = BackingPositionable.GetProjectedPosition2();
            Content.Animation currentAnimation = GameService.GetService<IContentService>().GetAsset<Content.Animation>(NormalizedAnimationName);
            bool hasDrawnMainAnim = false;

            if (AnimToSubAnim.ContainsKey(CurrentAnimationName)) {
                var subAnimations = AnimToSubAnim[CurrentAnimationName]
                    .Select(subAnim => GetSubAnimation(subAnim.Item1))
                    .OrderBy(animMeta => currentAnimation.GetHotspotDepthAtFrame(animMeta.Item2, CurrentFrame));
                foreach ((Content.Animation subAnim, string animHotspot) in subAnimations)
                {
                    (Vector2 hotspotOffset, float hotspotDepth) = currentAnimation.GetHotspot2AndDepthAtFrame(animHotspot, CurrentFrame);
                    if (!hasDrawnMainAnim && hotspotDepth >= 0)
                    {
                        currentAnimation.Draw(GameService.GetService<IRenderService>(), position, CurrentFrame, Tint, Angle: Angle, TargetAlphaStack: TargetAlphaStack, DuplicateToAlphaStack: DuplicateToAlphaStack);
                        hasDrawnMainAnim = true;
                    }
                    subAnim.Draw(
                        GameService.GetService<IRenderService>(),
                        position + hotspotOffset - currentAnimation.GetOriginAtFrame(CurrentFrame),
                        CurrentFrame,
                        Tint,
                        TargetAlphaStack: TargetAlphaStack,
                        DuplicateToAlphaStack: DuplicateToAlphaStack
                    );
                }
            }

            if (!hasDrawnMainAnim)
            {
                currentAnimation.Draw(
                    GameService.GetService<IRenderService>(), 
                    position, 
                    CurrentFrame, 
                    Tint, 
                    Angle: Angle, 
                    TargetAlphaStack: TargetAlphaStack, 
                    DuplicateToAlphaStack: DuplicateToAlphaStack
                );
            }
        }

        protected Content.Animation GetAsset<Animation>()
        {
            return GameService.GetService<IContentService>().GetAsset<Content.Animation>(NormalizedAnimationName);
        }

        protected (string, string) GetNormalizedSubAnimationDetails(string SubAnimation)
        {
            if (CurrentAnimationName == null || !AnimToSubAnim.ContainsKey(CurrentAnimationName))
            {
                return (null, null);
            }
            (string subAnimName, string animHotspot) = AnimToSubAnim[CurrentAnimationName].Find(l => l.Item1 == SubAnimation);
            return (subAnimName + "_" + Direction, animHotspot);
        }


        protected (Content.Animation, string) GetSubAnimation(string SubAnimation)
        {
            (string subAnimName, string animHotspot) = GetNormalizedSubAnimationDetails(SubAnimation);
            return (GameService.GetService<IContentService>().GetAsset<Content.Animation>(subAnimName), animHotspot);
        }

        public Vector3 GetSubAnimPosition(string SubAnimation)
        {
            Vector2 subActionCorner = GetSubAnimationOffset(SubAnimation);
            Vector3 position = BackingPositionable.GetPosition();
            return position + new Vector3(subActionCorner.X, 0, subActionCorner.Y);
        }

        protected Vector2 GetSubAnimationOffset(string SubAnimation)
        {
            Content.Animation currentAnimation = GetAsset<Content.Animation>();
            (Content.Animation subAnim, string animHotspot) = GetSubAnimation(SubAnimation);
            Vector2 currentFrameOrigin = currentAnimation.GetOriginAtFrame(CurrentFrame);
            Vector2 hotspotOffset = currentAnimation.GetHotspot2AtFrame(animHotspot, CurrentFrame);
            Vector2 subAnimOrigin = subAnim.GetOriginAtFrame(CurrentFrame);

            return hotspotOffset - currentFrameOrigin - subAnimOrigin;
        }

        public Vector3 GetHotspotPosition(string Hotspot)
        {
            return GetAsset<Content.Animation>().GetHotspotAtFrame(Hotspot, CurrentFrame);
        }
    }
}
