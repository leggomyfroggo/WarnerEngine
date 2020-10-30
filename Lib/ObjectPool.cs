using System;
using System.Collections.Generic;

namespace WarnerEngine.Lib
{
    public class ObjectPool<T> where T : new()
    {
        private Stack<T> pool;
        private int poolSize;
        private Action<T> returnAction;

        public ObjectPool(int PoolSize = 100, Action<T> ReturnAction = null)
        {
            poolSize = PoolSize;
            pool = new Stack<T>(poolSize);
            returnAction = ReturnAction;
            for (int i = 0; i < PoolSize; i++)
            {
                pool.Push(new T());
            }
        }

        public T Rent()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            return new T();
        }

        public void Return(T ReturnedObject)
        {
            if (pool.Count < poolSize)
            {
                if (returnAction != null)
                {
                    returnAction(ReturnedObject);
                }
                pool.Push(ReturnedObject);
            }
        }
    }
}
