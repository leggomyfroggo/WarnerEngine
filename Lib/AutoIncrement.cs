using Microsoft.Xna.Framework;

namespace WarnerEngine.Lib
{
    public class AutoIncrement
    {
        private int stepSize;
        private int internalValue;
        public int InternalValue { 
            get
            {
                return internalValue;
            }
            private set
            {
                internalValue = value;
                StringRepresentation = internalValue.ToString();
            }
        }
        public string StringRepresentation { get; private set; }

        public AutoIncrement(int StepSize, int InitialValue)
        {
            stepSize = StepSize;
            InternalValue = InitialValue;
        }

        public bool IncrementTowardTarget(int Target)
        {
            if (Target == InternalValue)
            {
                return false;
            }
            else if (Target > InternalValue)
            {
                InternalValue = MathHelper.Clamp(InternalValue + stepSize, InternalValue, Target);
            }
            else
            {
                InternalValue = MathHelper.Clamp(InternalValue - stepSize, Target, InternalValue);
            }
            return true;
        }
    }
}
