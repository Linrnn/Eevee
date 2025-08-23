using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public readonly struct ReadOnlyArray<T>
    {
        #region Feild/Constructor
        private readonly T[] _ptr;
        private readonly int _start;
        public readonly int Count;

        public ReadOnlyArray(T[] ptr)
        {
            Assert.NotNull<ArgumentNullException, DiagnosisArgs>(ptr, nameof(ptr), "is null!");

            _ptr = ptr;
            _start = 0;
            Count = ptr.Length;
        }
        public ReadOnlyArray(T[] ptr, int count)
        {
            Assert.NotNull<ArgumentNullException, DiagnosisArgs>(ptr, nameof(ptr), "is null!");
            Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<int, int>, int>(ptr.Length, count, nameof(count), "Length:{0} <= Count:{1}!", new DiagnosisArgs<int, int>(ptr.Length, count));

            _ptr = ptr;
            _start = 0;
            Count = count;
        }
        public ReadOnlyArray(T[] ptr, int start, int count)
        {
            Assert.NotNull<ArgumentNullException, DiagnosisArgs>(ptr, nameof(ptr), "is null!");
            Assert.Less<ArgumentOutOfRangeException, DiagnosisArgs<int, int>, int>(start, ptr.Length, nameof(start), "Start:{0} < Length:{1}!", new DiagnosisArgs<int, int>(start, ptr.Length));
            Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<int, int, int>, int>(ptr.Length - start, count, nameof(count), "Length:{0} <= Start:{1} + Count:{2}!", new DiagnosisArgs<int, int, int>(ptr.Length, start, count));

            _ptr = ptr;
            _start = start;
            Count = count;
        }
        #endregion

        #region Method
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
            Assert.Range<ArgumentOutOfRangeException, DiagnosisArgs<int, int, int>, int>(index, 0, Count - _start - 1, nameof(index), "get fail, Index:{0} out of range, Start:{1}, Count:{2}", new DiagnosisArgs<int, int, int>(index, _start, Count));
            return _ptr[_start + index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T RefGet(int index)
        {
            Assert.Range<ArgumentOutOfRangeException, DiagnosisArgs<int, int, int>, int>(index, 0, Count - _start - 1, nameof(index), "get fail, Index:{0} out of range, Start:{1}, Count:{2}", new DiagnosisArgs<int, int, int>(index, _start, Count));
            return ref _ptr[_start + index];
        }

        public ReadOnlySpan<T> AsSpan() => new(_ptr, _start, Count);
        public ReadOnlySpan<T>.Enumerator GetEnumerator() => new ReadOnlySpan<T>(_ptr, _start, Count).GetEnumerator();
        #endregion
    }
}