using System;

namespace Eevee.Utils
{
    /// <summary>
    /// “Array.Sort”存在GC，需要“Comparison”避免GC
    /// </summary>
    public readonly struct Comparer
    {
        private static Comparison<bool> _bool;
        public static Comparison<bool> Bool => _bool ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<byte> _byte;
        public static Comparison<byte> Byte => _byte ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<sbyte> _sbyte;
        public static Comparison<sbyte> Sbyte => _sbyte ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<short> _short;
        public static Comparison<short> Short => _short ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<ushort> _ushort;
        public static Comparison<ushort> Ushort => _ushort ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<int> _int;
        public static Comparison<int> Int => _int ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<uint> _uint;
        public static Comparison<uint> Uint => _uint ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<long> _long;
        public static Comparison<long> Long => _long ??= (lhs, rhs) => lhs.CompareTo(rhs);

        private static Comparison<ulong> _ulong;
        public static Comparison<ulong> Ulong => _ulong ??= (lhs, rhs) => lhs.CompareTo(rhs);
    }
}