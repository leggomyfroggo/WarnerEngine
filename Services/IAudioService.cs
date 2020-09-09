using Microsoft.Xna.Framework;

using WarnerEngine.Lib.Components;

namespace WarnerEngine.Services
{
    public interface IAudioService : IService
    {
        BackingBox TrackedObject { get; }
        IAudioService PlaySoundEffect(string Key, Vector3? SourcePosition = null);
        IAudioService SetTrackedObject(BackingBox TrackedObject);
    }
}
