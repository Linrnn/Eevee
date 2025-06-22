using Eevee.Define;
using Eevee.Fixed;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树元素
    /// </summary>
    public readonly struct QuadElement : IEquatable<QuadElement>, IComparable<QuadElement>, IFormattable
    {
        #region 字段/构造方法
        public readonly int Index;
        public readonly AABB2DInt Shape;

        public QuadElement(int index, in AABB2DInt shape)
        {
            Index = index;
            Shape = shape;
        }
        #endregion

        #region 运算符重载
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in QuadElement lhs, in QuadElement rhs) => lhs.Index == rhs.Index && lhs.Shape == rhs.Shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in QuadElement lhs, in QuadElement rhs) => lhs.Index != rhs.Index || lhs.Shape != rhs.Shape;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is QuadElement other && this == other;
        public override int GetHashCode() => Index ^ Shape.GetHashCode();
        public int CompareTo(QuadElement other)
        {
            int match0 = Index.CompareTo(other.Index);
            if (match0 != 0)
                return match0;

            int match1 = Shape.CompareTo(other.Shape);
            if (match1 != 0)
                return match1;

            return 0;
        }
        public bool Equals(QuadElement other) => other == this;

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider) => $"Index:{Index.ToString(format, provider)}, AABB:{Shape.ToString(format, provider)}";
        #endregion
    }
}