using WarnerEngine.Lib.Components.Combat;

namespace WarnerEngine.Lib.Entities
{
    public interface ITrackable : IDraw
    {
        TrackingPoints TrackingPoints { get; }
    }
}
