using Microsoft.Xna.Framework;

using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public class RandomRangeCone: IRandomRange<Vector3>
    {
        private Vector3 _direction;
        private IRandomRange<float> _spread;
        private IRandomRange<float> _speed;

        public RandomRangeCone(Vector3 Direction, float Spread, IRandomRange<float> Speed)
        {
            _direction = Direction;
            _direction.Normalize();
            _spread = new RandomRangeFloat(-Spread, Spread);
            _speed = Speed;
        }

        public Vector3 Sample()
        {
            Vector3 baseDirection = Vector3.Transform(
                _direction, 
                Matrix.CreateRotationX(_spread.Sample()) * Matrix.CreateRotationY(_spread.Sample()) * Matrix.CreateRotationZ(_spread.Sample())
            );
            return baseDirection * _speed.Sample();
        }
    }
}
