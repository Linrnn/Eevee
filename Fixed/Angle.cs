using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的角度
    /// </summary>
    [Serializable]
    public struct Angle : IEquatable<Angle>, IComparable<Angle>, IFormattable
    {
        #region 字段/构造函数
        public Fixed64 Value; // 角度角，X轴正方向为0°，逆时针递增，值域：[0, 360°)

        public Angle(Fixed64 deg)
        {
            Check.Deg0To360(deg, nameof(deg));
            Value = deg;
        }
        #endregion

        #region 基础方法
        public readonly Vector2D Direction() => new(Maths.CosDeg(Value), Maths.SinDeg(Value));
        #endregion

        #region 隐式转换/运算符重载
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Angle(Fixed64 value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Fixed64(Angle value) => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Angle lhs, Angle rhs) => lhs.Value == rhs.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Angle lhs, Angle rhs) => lhs.Value != rhs.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Angle lhs, Angle rhs) => lhs.Value >= rhs.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Angle lhs, Angle rhs) => lhs.Value <= rhs.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Angle lhs, Angle rhs) => lhs.Value > rhs.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Angle lhs, Angle rhs) => lhs.Value < rhs.Value;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Angle other && this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => Value.GetHashCode();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Angle other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(Angle other) => Value.CompareTo(other.Value);

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => Value.ToString(format, provider);
        #endregion
    }
}