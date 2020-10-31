using System;
using System.Buffers;

namespace WarnerEngine.Lib
{
    public class DisposableList<T> : IDisposable
    {
        public static ObjectPool<DisposableList<T>> Shared;

        private T[] backingArray;

        private int count;
        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsDisposed { get; private set; }

        public T this[int i]
        {
            get { return backingArray[i]; }
        }

        static DisposableList()
        {
            Shared = new ObjectPool<DisposableList<T>>();
        }

        public DisposableList()
        {
            count = -1;
            IsDisposed = true;
        }

        public DisposableList(int MaxSize) 
        {
            Initialize(MaxSize);
        }

        public DisposableList<T> Initialize(int MaxSize)
        {
            IsDisposed = false;
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
            IsDisposed = true;
            ArrayPool<T>.Shared.Return(backingArray);
            Shared.Return(this);
        }
    }
}
