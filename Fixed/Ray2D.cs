using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的二维射线
    /// </summary>
    [Serializable]
    public struct Ray2D : IEquatable<Ray2D>, IComparable<Ray2D>, IFormattable
    {
        #region 字段/初始化
        public Vector2D Origin;
        public Vector2D Direction; // 必须是单位向量

        public Ray2D(in Vector2D origin, in Vector2D direction)
        {
            Origin = origin;
            Direction = direction.Normalized();
        }
        #endregion

        #region 基础方法
        public readonly Vector2D GetPoint(Fixed64 distance) => Origin + Direction * distance;
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_STANDALONE
        public static implicit operator Ray2D(in UnityEngine.Ray2D value) => new(value.origin, value.direction);
        public static explicit operator UnityEngine.Ray2D(in Ray2D value) => new((UnityEngine.Vector2)value.Origin, (UnityEngine.Vector2)value.Direction);
#endif

        public static bool operator ==(in Ray2D lhs, in Ray2D rhs) => lhs.Origin == rhs.Origin && lhs.Direction == rhs.Direction;
        public static bool operator !=(in Ray2D lhs, in Ray2D rhs) => lhs.Origin != rhs.Origin || lhs.Direction != rhs.Direction;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Ray2D other && this == other;
        public readonly override int GetHashCode() => Origin.GetHashCode() ^ Direction.GetHashCode();
        public readonly bool Equals(Ray2D other) => this == other;
        public readonly int CompareTo(Ray2D other)
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