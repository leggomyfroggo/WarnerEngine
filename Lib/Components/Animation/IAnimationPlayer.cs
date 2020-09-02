using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ProjectWarnerShared.Lib.Components.Animation
{
    public interface IAnimationPlayer
    {
        BackingBox BackingPositionable { get; set; }
        string CurrentAnimationName { get; }
        Enums.Direction Direction { get; }
        bool DidChangeFrame { get; }
        float SpeedMultiplier { get; set; }
        int CurrentFrame { get; }
        float FrameTime { get; }
        bool IsPlaying { get; }
        Dictionary<string, List<(string, string)>> AnimToSubAnim { get; }

        IAnimationPlayer SetAnimation(string Name, bool ResetState = true);
        IAnimationPlayer SetDirection(Enums.Direction Direction);
        IAnimationPlayer Play();
        IAnimationPlayer Pause();
        IAnimationPlayer PlayFromBeginning();
        IAnimationPlayer Stop();
        IAnimationPlayer SetAnimToSubAnim(string AnimName, string SubAnimName, string AnimHotspot);

        float GetProgress();
        void PreDraw(float DT);
        void Draw(Color? Tint = null, float Angle = 0f, string TargetAlphaStack = null, bool DuplicateToAlphaStack = false);

        Vector3 GetSubAnimPosition(string SubAnimatino);
        Vector3 GetHotspotPosition(string Hotspot);
    }
}
