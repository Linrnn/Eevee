using System.Collections;
using System.Collections.Generic;

namespace Eevee.Pool
{
    public sealed class CollectionPoolValue
    {
        public const int ConstMaxCount = 128;
        public int MaxCount = ConstMaxCount;
        internal ICollection Pool;

        internal CollectionPoolValue(ICollection pool) => Pool = pool;
        internal Stack<T> GetPool<T>() => Pool as Stack<T>;
        internal bool IsFull() => Pool.Count >= MaxCount;

        internal static Stack<T> NewPool<T>() => new();
    }
}