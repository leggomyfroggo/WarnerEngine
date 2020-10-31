using System;
using System.Buffers;

namespace WarnerEngine.Lib
{
    public class DisposableArray<T> : IDisposable
    {
        public static ObjectPool<DisposableArray<T>> Shared;

        private T[] backingArray;

        private int length;
        public int Length
        {
            get
            {
                return length;
            }
        }

        public bool IsDisposed { get; private set; }

        public T this[int i]
        {
            get { return backingArray[i]; }
            set 
            { 
                if (i >= length)
                {
                    throw new IndexOutOfRangeException();
                }
                backingArray[i] = value; 
            }
        }

        static DisposableArray()
        {
            Shared = new ObjectPool<DisposableArray<T>>();
        }

        public DisposableArray()
        {
            length = 0;
            IsDisposed = true;
        }

        public DisposableArray(int Size)
        {
            Initialize(Size);
        }

        public DisposableArray<T> Initialize(int Size)
        {
            IsDisposed = false;
            backingArray = ArrayPool<T>.Shared.Rent(Size);
            length = Size;
            return this;
        }

        public void Dispose()
        {
            IsDisposed = true;
            ArrayPool<T>.Shared.Return(backingArray);
            Shared.Return(this);
        }
    }
}
