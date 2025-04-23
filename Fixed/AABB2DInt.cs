using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 2D轴对齐包围盒
    /// </summary>
    public readonly struct AABB2DInt : IEquatable<AABB2DInt>, IComparable<AABB2DInt>, IFormattable
    {
        #region 字段/构造方法
        public readonly int X; // 包围盒中心点（X）
        public readonly int Y; // 包围盒中心点（Y）
        public readonly int W; // 半宽
        public readonly int H; // 半高

        public AABB2DInt(int x, int y, int e)
        {
            X = x;
            Y = y;
            W = e;
            H = e;
        }
        public AABB2DInt(int left, int top, int right, int bottom)
        {
            X = right + left >> 1;
            Y = top + bottom >> 1;
            W = right - left >> 1;
            H = top - bottom >> 1;
        }

        public AABB2DInt(Vector2DInt center, int extent)
        {
            X = center.X;
            Y = center.Y;
            W = extent;
            H = extent;
        }
        public AABB2DInt(Vector2DInt center, Fixed64 extent)
        {
            int e = (int)extent;
            X = center.X;
            Y = center.Y;
            W = e;
            H = e;
        }
        public AABB2DInt(in Vector2D center, Fixed64 extent)
        {
            int e = (int)extent;

            X = (int)center.X;
            Y = (int)center.Y;
            W = e;
            H = e;
        }

        public AABB2DInt(Vector2DInt center, Vector2DInt extent)
        {
            X = center.X;
            Y = center.Y;
            W = extent.X;
            H = extent.Y;
        }
        public AABB2DInt(in Vector2D center, in Vector2D extent)
        {
            X = (int)center.X;
            Y = (int)center.Y;
            W = (int)extent.X;
            H = (int)extent.Y;
        }
        #endregion

        #region 基础方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left() => X - W; // 左
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Right() => X + W; // 右
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Bottom() => Y - H; // 下
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Top() => Y + H; // 上

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Center() => new(X, Y); // 中心点
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Size() => new(W << 1, H << 1); // 尺寸
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt HalfSize() => new(W, H); // 一半的尺寸
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Min() => new(Left(), Bottom()); // 左下角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Max() => new(Right(), Top()); // 右上角

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt LeftBottom() => new(Left(), Bottom()); // 左下角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt RightBottom() => new(Right(), Bottom()); // 右下角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt RightTop() => new(Right(), Top()); // 右上角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt LeftTop() => new(Left(), Top()); // 左上角 

        public bool ContainPoint(Vector2DInt point) => Left() <= point.X && Right() >= point.X && Bottom() <= point.Y && Top() >= point.Y;
        public bool ContainPoint(in Vector2D point) => Left() <= point.X && Right() >= point.X && Bottom() <= point.Y && Top() >= point.Y;
        public bool Contain(in AABB2DInt other) => Left() <= other.Left() && Right() >= other.Right() && Bottom() <= other.Bottom() && Top() >= other.Top(); // 是否包含aabb

        public bool Intersect_Box_Circle(in AABB2DInt circle) => IntersectBoxAndCircle(in this, in circle); // 检测aabb与圆是否相交
        public bool Intersect_Circle_Box(in AABB2DInt box) => IntersectBoxAndCircle(in box, in this); // 检测圆与aabb是否相交
        public bool Intersect_Circle_Circle(in AABB2DInt other) // 检测圆与圆是否相交
        {
            int w = W + other.W;
            int h = H + other.H;

            int x = X - other.X;
            int y = Y - other.Y;

            return w * h >= x * x + y * y;
        }
        public bool Intersect_Box_Box(in AABB2DInt other) => Math.Abs(X - other.X) < W + other.W && Math.Abs(Y - other.Y) < H + other.H; // 检测aabb与aabb是否相交

        private bool IntersectBoxAndCircle(in AABB2DInt box, in AABB2DInt circle)
        {
            ulong x = (ulong)Math.Max(Math.Abs(box.X - circle.X) - box.W, 0);
            ulong y = (ulong)Math.Max(Math.Abs(box.Y - circle.Y) - box.H, 0);
            return x * x + y * y <= (ulong)circle.W * (ulong)circle.H;
        }

        public AABB2DInt Intersect(in AABB2DInt other) => new(Math.Max(Left(), other.Left()), Math.Min(Top(), other.Top()), Math.Min(Right(), other.Right()), Math.Max(Bottom(), other.Bottom()));
        #endregion

        #region 运算符重载
        public static bool operator ==(in AABB2DInt left, in AABB2DInt right) => left.X == right.X && left.Y == right.Y && left.W == right.W && left.H == right.H;
        public static bool operator !=(in AABB2DInt left, in AABB2DInt right) => left.X != right.X || left.Y != right.Y || left.W != right.W || left.H != right.H;
        #endregion

        #region 继承/重载
        public override bool Equals(object other) => other is AABB2DInt aabb && aabb == this;
        public override int GetHashCode() => X ^ Y ^ W ^ H;
        public bool Equals(AABB2DInt other) => this == other;
        public int CompareTo(AABB2DInt other)
        {
            int match0 = X.CompareTo(other.X);
            if (match0 != 0)
                return match0;

            int match1 = X.CompareTo(other.Y);
            if (match1 != 0)
                return match1;

            int match2 = X.CompareTo(other.W);
            if (match2 != 0)
                return match2;

            int match3 = X.CompareTo(other.H);
            if (match3 != 0)
                return match3;

            return 0;
        }

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider) => $"Center:{Center().ToString(format, provider)}, Size:{Size().ToString(format, provider)}";
        #endregion
    }
}