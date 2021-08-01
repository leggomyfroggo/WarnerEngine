using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public class RandomRangeFloat : IRandomRange<float>
    {
        private float _lowerBound;
        private float _upperBound;

        public RandomRangeFloat(float LowerBound, float UpperBound)
        {
            _lowerBound = LowerBound;
            _upperBound = UpperBound;
        }

        public RandomRangeFloat(float UnitBound)
        {
            _lowerBound = UnitBound;
            _upperBound = UnitBound;
        }

        public float Sample()
        {
            if (_lowerBound == _upperBound)
            {
                return _lowerBound;
            }
            var rand = GameService.GetService<IStateService>().GetGlobalRandom();
            return (float)rand.NextDouble() * (_upperBound - _lowerBound) + _lowerBound;
        }
    }
}
