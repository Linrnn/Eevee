using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 圆形
    /// </summary>
    public readonly struct CircleInt : IEquatable<CircleInt>, IComparable<CircleInt>, IFormattable
    {
        #region 字段/构造方法
        public readonly int X; // 中心点（X）
        public readonly int Y; // 中心点（Y）
        public readonly int R; // 半径

        public CircleInt(int x, int y, int r)
        {
            Check.Extents(r, nameof(r));

            X = x;
            Y = y;
            R = r;
        }
        public CircleInt(Vector2DInt center, int radius)
        {
            Check.Extents(radius, nameof(radius));

            X = center.X;
            Y = center.Y;
            R = radius;
        }
        #endregion

        #region 基础方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left() => X - R; // 左
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Right() => X + R; // 右
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Bottom() => Y - R; // 下
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Top() => Y + R; // 上

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Center() => new(X, Y); // 中心点
        public int Diameter() => R << 1; // 直径
        public Fixed64 Perimeter() => (R << 1) * Maths.Pi; // 周长
        public Fixed64 Area() => R * R * Maths.Pi; // 面积

        public int SqrDistance(Vector2DInt other) // 圆心到点距离的平方
        {
            int x = X - other.X;
            int y = Y - other.Y;
            return x * x + y * y;
        }
        public int SqrDistance(in CircleInt other) // 圆心之间距离的平方
        {
            int x = X - other.X;
            int y = Y - other.Y;
            return x * x + y * y;
        }
        public Fixed64 Distance(Vector2DInt other) => ((Fixed64)SqrDistance(other)).Sqrt(); // 圆心到点的距离
        public Fixed64 Distance(in CircleInt other) => ((Fixed64)SqrDistance(other)).Sqrt(); // 圆心之间的距离
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static implicit operator Circle(in CircleInt value) => new(value.X, value.Y, value.R);
        public static explicit operator CircleInt(in Circle value) => new((int)value.X, (int)value.Y, (int)value.R);

        public static bool operator ==(in CircleInt lhs, in CircleInt rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.R == rhs.R;
        public static bool operator !=(in CircleInt lhs, in CircleInt rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.R != rhs.R;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is CircleInt other && this == other;
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ R.GetHashCode();
        public bool Equals(CircleInt other) => this == other;
        public int CompareTo(CircleInt other)
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