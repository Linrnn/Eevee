using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public readonly unsafe struct ReadOnlyArray<T> where T : unmanaged, IEquatable<T>
    {
        #region Feild/Constructor
        private readonly T* _ptr;
        public readonly int Count;

        public ReadOnlyArray(T* ptr, int count)
        {
            Assert.NotNull<ArgumentNullException, AssertArgs, T>(ptr, nameof(ptr), "is null!");
            _ptr = ptr;
            Count = count;
        }
        #endregion

        #region Other
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Assert.Range<ArgumentOutOfRangeException, AssertArgs<int, int>, int>(index, 0, Count - 1, nameof(index), "get fail, index:{0} out of range [0, {1})", new AssertArgs<int, int>(index, Count));
                return ref _ptr[index];
            }
        }

        public ReadOnlySpan<T> AsSpan() => new(_ptr, Count);
        public ReadOnlySpan<T>.Enumerator GetEnumerator() => new ReadOnlySpan<T>(_ptr, Count).GetEnumerator();
        #endregion
    }
}