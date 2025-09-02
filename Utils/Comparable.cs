using System;

namespace Eevee.Utils
{
    /// <summary>
    /// “Array.Sort”存在GC，需要“Comparison”避免GC
    /// </summary>
    public readonly struct Comparable<T> where T : IComparable<T>
    {
        public static readonly Comparison<T> Default = (lhs, rhs) => lhs.CompareTo(rhs);
    }
}