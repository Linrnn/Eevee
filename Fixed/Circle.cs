using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 圆形
    /// </summary>
    public readonly struct Circle : IEquatable<Circle>, IComparable<Circle>, IFormattable
    {
        #region 字段/构造方法
        public readonly Fixed64 X; // 中心点（X）
        public readonly Fixed64 Y; // 中心点（Y）
        public readonly Fixed64 R; // 半径

        public Circle(Fixed64 x, Fixed64 y, Fixed64 r)
        {
            Check.Extents(r, nameof(r));

            X = x;
            Y = y;
            R = r;
        }
        public Circle(in Vector2D center, Fixed64 radius)
        {
            Check.Extents(radius, nameof(radius));

            X = center.X;
            Y = center.Y;
            R = radius;
        }
        #endregion

        #region 基础方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Left() => X - R; // 左
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Right() => X + R; // 右
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Bottom() => Y - R; // 下
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Top() => Y + R; // 上

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Center() => new(X, Y); // 中心点
        public Fixed64 Diameter() => R << 1; // 直径
        public Fixed64 Perimeter() => (R << 1) * Maths.Pi; // 周长
        public Fixed64 Area() => R.Sqr() * Maths.Pi; // 面积

        public Fixed64 SqrDistance(in Vector2D other) => (X - other.X).Sqr() + (Y - other.Y).Sqr(); // 圆心到点距离的平方
        public Fixed64 SqrDistance(in Circle other) => (X - other.X).Sqr() + (Y - other.Y).Sqr(); // 圆心之间距离的平方
        public Fixed64 Distance(in Vector2D other) => SqrDistance(other).Sqrt(); // 圆心到点的距离
        public Fixed64 Distance(in Circle other) => SqrDistance(other).Sqrt(); // 圆心之间的距离
        #endregion

        #region 运算符重载
        public static bool operator ==(in Circle lhs, in Circle rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.R == rhs.R;
        public static bool operator !=(in Circle lhs, in Circle rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.R != rhs.R;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is Circle other && this == other;
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ R.GetHashCode();
        public bool Equals(Circle other) => this == other;
        public int CompareTo(Circle other)
        {
            int match0 = X.CompareTo(other.X);
            if (match0 != 0)
                return match0;

            int match1 = Y.CompareTo(other.Y);
            if (match1 != 0)
                return match1;

            int match2 = R.CompareTo(other.R);
            if (match2 != 0)
                return match2;

            return 0;
        }

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider) => $"({X.ToString(format, provider)}, {Y.ToString(format, provider)}, {R.ToString(format, provider)})";
        #endregion
    }
}