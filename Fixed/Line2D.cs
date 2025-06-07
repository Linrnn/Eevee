using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的2D直线
    /// </summary>
    [Serializable]
    public struct Line2D : IEquatable<Line2D>, IComparable<Line2D>, IFormattable
    {
        #region 字段/初始化
        public Vector2D Origin;
        public Vector2D Direction; // 必须是单位向量

        public Line2D(in Vector2D origin, in Vector2D direction)
        {
            Check.NotZero(in direction);
            Check.Normal(in direction);

            Origin = origin;
            Direction = direction;
        }
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static bool operator ==(in Line2D lhs, in Line2D rhs) => lhs.Origin == rhs.Origin && lhs.Direction == rhs.Direction;
        public static bool operator !=(in Line2D lhs, in Line2D rhs) => lhs.Origin != rhs.Origin || lhs.Direction != rhs.Direction;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Line2D other && this == other;
        public readonly override int GetHashCode() => Origin.GetHashCode() ^ Direction.GetHashCode();
        public readonly bool Equals(Line2D other) => this == other;
        public readonly int CompareTo(Line2D other)
        {
            int match0 = Origin.CompareTo(Origin);
            if (match0 != 0)
                return match0;

            int match1 = Direction.CompareTo(Direction);
            if (match1 != 0)
                return match1;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"[Origin:{Origin.ToString(format, provider)}, Direction:{Direction.ToString(format, provider)})]";
        #endregion
    }
}