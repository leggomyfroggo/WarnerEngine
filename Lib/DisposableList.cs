using System;
using System.Buffers;
using System.Collections.Generic;

namespace WarnerEngine.Lib
{
    public class DisposableArray<T> : IDisposable
    {
        public static ObjectPool<DisposableArray<T>> Shared;

        private T[] backingArray;

        private int count;
        public int Count
        {
            get
            {
                return count;
            }
        }

        public T this[int i]
        {
            get { return backingArray[i]; }
        }

        static DisposableArray()
        {
            Shared = new ObjectPool<DisposableArray<T>>();
        }

        public DisposableArray()
        {
            count = -1;
        }

        public DisposableArray(int MaxSize) 
        {
            Initialize(MaxSize);
        }

        public DisposableArray<T> Initialize(int MaxSize)
        {
            backingArray = ArrayPool<T>.Shared.Rent(MaxSize);
            count = 0;
            return this;
        }

        public void Add(T TObject)
        {
            backingArray[count++] = TObject;
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(backingArray);
            Shared.Return(this);
        }
    }
}
