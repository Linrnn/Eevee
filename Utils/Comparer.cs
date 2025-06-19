using System;

namespace Eevee.Utils
{
    /// <summary>
    /// Array.Sort存在GC，所以需要委托比较器避免GC
    /// </summary>
    public readonly struct Comparer
    {
        private static Comparison<int> _int32;
        public static Comparison<int> Int32 => _int32 ??= (lhs, rhs) => lhs - rhs;
    }
}