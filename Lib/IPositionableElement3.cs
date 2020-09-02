using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib
{
    public interface IPositionableElement3
    {
        Vector3 GetPosition();
        Vector3 GetCenterPoint();
        Vector2 GetProjectedPosition2();
        Vector2 GetProjectedCenterPoint2();
    }
}
