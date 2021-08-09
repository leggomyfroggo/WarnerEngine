using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.Bindings
{
    public abstract class ABinding<TLeft, TRight> : IBinding where TLeft : ISceneEntity where TRight : ISceneEntity
    {
        protected TLeft Left;
        protected TRight Right;

        public ISceneEntity GetLeftEntity()
        {
            return Left;
        }

        public ISceneEntity GetRightEntity()
        {
            return Right;
        }

        public ABinding(TLeft Left, TRight Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public abstract bool ProcessBinding();

        public abstract void OnLeftRemoved();
    }
}
