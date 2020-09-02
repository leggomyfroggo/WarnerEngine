using Microsoft.Xna.Framework;

namespace ProjectWarnerShared.Lib.Components.Animation.Test
{
    public class TestAnimationPlayer : IAnimationPlayer
    {
        public BackingBox BackingPositionable { get; set; }

        public string CurrentAnimationName { get; set; }

        public Enums.Direction Direction { get; set; }

        public bool DidChangeFrame { get; set; }

        public float SpeedMultiplier { get; set; }

        public int CurrentFrame { get; set; }

        public float FrameTime { get; set; }

        public bool IsPlaying { get; set; }

        public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<(string, string)>> AnimToSubAnim { get; set; }

        public void Draw(Color? Tint = null, float Angle = 0, string TargetAlphaStack = null, bool DuplicateToAlphaStack = false) { }

        public Vector3 GetHotspotPosition(string Hotspot)
        {
            return Vector3.Zero;
        }

        public float GetProgress()
        {
            return 0;
        }

        public Vector3 GetSubAnimPosition(string SubAnimatino)
        {
            return Vector3.Zero;
        }

        public IAnimationPlayer Pause()
        {
            IsPlaying = false;
            return this;
        }

        public IAnimationPlayer Play()
        {
            IsPlaying = true;
            return this;
        }

        public IAnimationPlayer PlayFromBeginning()
        {
            IsPlaying = true;
            return this;
        }

        public void PreDraw(float DT) { }

        public IAnimationPlayer SetAnimation(string Name, bool ResetState = true)
        {
            CurrentAnimationName = Name;
            return this;
        }

        public IAnimationPlayer SetAnimToSubAnim(string AnimName, string SubAnimName, string AnimHotspot)
        {
            return this;
        }

        public IAnimationPlayer SetDirection(Enums.Direction Direction)
        {
            this.Direction = Direction;
            return this;
        }

        public IAnimationPlayer Stop()
        {
            IsPlaying = false;
            return this;
        }
    }
}
