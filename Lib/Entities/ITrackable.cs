using ProjectWarnerShared.Lib.Components.Combat;

namespace ProjectWarnerShared.Lib.Entities
{
    public interface ITrackable : IDraw
    {
        TrackingPoints TrackingPoints { get; }
    }
}
