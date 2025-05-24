using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 二维轴对齐包围盒
    /// </summary>
    public readonly struct AABB2D : IEquatable<AABB2D>, IComparable<AABB2D>, IFormattable
    {
        #region 字段/构造方法
        public readonly Fixed64 X; // 中心点（X）
        public readonly Fixed64 Y; // 中心点（Y）
        public readonly Fixed64 W; // 半宽
        public readonly Fixed64 H; // 半高

        public AABB2D(Fixed64 x, Fixed64 y, Fixed64 e)
        {
            Check.Extents(e, nameof(e));

            X = x;
            Y = y;
            W = e;
            H = e;
        }
        public AABB2D(Fixed64 x, Fixed64 y, Fixed64 w, Fixed64 h)
        {
            Check.Extents(w, nameof(w));
            Check.Extents(h, nameof(h));

            X = x;
            Y = y;
            W = w;
            H = h;
        }
        public AABB2D(Fixed64 x, Fixed64 y, in Vector2D e)
        {
            Check.Extents(e.X, "e.x");
            Check.Extents(e.Y, "e.y");

            X = x;
            Y = y;
            W = e.X;
            H = e.Y;
        }
        public AABB2D(in Vector2D center, Fixed64 extents)
        {
            Check.Extents(extents, nameof(extents));

            X = center.X;
            Y = center.Y;
            W = extents;
            H = extents;
        }
        public AABB2D(in Vector2D center, Fixed64 width, Fixed64 height)
        {
            Check.Extents(width, nameof(width));
            Check.Extents(height, nameof(height));

            X = center.X;
            Y = center.Y;
            W = width;
            H = height;
        }
        public AABB2D(in Vector2D center, in Vector2D extents)
        {
            Check.Extents(extents.X, "extents.x");
            Check.Extents(extents.Y, "extents.y");

            X = center.X;
            Y = center.Y;
            W = extents.X;
            H = extents.Y;
        }

        public static AABB2D Create(Fixed64 left, Fixed64 top, Fixed64 right, Fixed64 bottom) => new(right + left >> 1, top + bottom >> 1, right - left >> 1, top - bottom >> 1);
        #endregion

        #region 中心点/尺寸/边界
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Left() => X - W; // 左
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Right() => X + W; // 右
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Bottom() => Y - H; // 下
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Top() => Y + H; // 上

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Center() => new(X, Y); // 中心点
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Size() => new(W << 1, H << 1); // 尺寸
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D HalfSize() => new(W, H); // 一半尺寸
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Min() => LeftBottom();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Max() => RightTop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D LeftBottom() => new(Left(), Bottom()); // 左下角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D RightBottom() => new(Right(), Bottom()); // 右下角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D RightTop() => new(Right(), Top()); // 右上角
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D LeftTop() => new(Left(), Top()); // 左上角

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2D LeftBottomAABB() // 左下AABB，原先1/4的AABB
        {
            var w = W >> 1;
            var h = H >> 1;
            return new AABB2D(X - w, Y - h, w, h);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2D RightBottomAABB() // 右下AABB，原先1/4的AABB
        {
            var w = W >> 1;
            var h = H >> 1;
            return new AABB2D(X + w, Y - h, w, h);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2D RightTopAABB() // 右上AABB，原先1/4的AABB
        {
            var w = W >> 1;
            var h = H >> 1;
            return new AABB2D(X + w, Y + h, w, h);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2D LeftTopAABB() // 左上AABB，原先1/4的AABB
        {
            var w = W >> 1;
            var h = H >> 1;
            return new AABB2D(X - w, Y + h, w, h);
        }
        #endregion

        #region 基础方法
        public bool Contain(Vector2D other)
        {
            var dx = (other.X - X).Abs();
            if (dx > W)
                return false;

            var dy = (other.Y - Y).Abs();
            if (dy > H)
                return false;

            return true;
        }
        public bool Contain(in Circle other) => Left() <= other.Left() && Right() >= other.Right() && Bottom() <= other.Bottom() && Top() >= other.Top(); // 包含aabb
        public bool Contain(in AABB2D other) => Left() <= other.Left() && Right() >= other.Right() && Bottom() <= other.Bottom() && Top() >= other.Top(); // 包含aabb

        public bool Intersect(in Circle other) // 检测aabb与圆是否相交
        {
            var x = Fixed64.Max((X - other.X).Abs() - W, Fixed64.Zero);
            var y = Fixed64.Max((Y - other.Y).Abs() - H, Fixed64.Zero);
            return x.Sqr() + y.Sqr() <= other.R.Sqr();
        }
        public bool Intersect(in AABB2D other) => (X - other.X).Abs() < W + other.W && (Y - other.Y).Abs() < H + other.H; // 检测aabb与aabb是否相交
        public bool Intersect(in AABB2D other, out AABB2D intersect) // AABB的交集
        {
            var left = Fixed64.Max(Left(), other.Left());
            var right = Fixed64.Min(Right(), other.Right());
            if (left > right)
            {
                intersect = default;
                return false;
            }

            var bottom = Fixed64.Max(Bottom(), other.Bottom());
            var top = Fixed64.Min(Top(), other.Top());
            if (bottom > top)
            {
                intersect = default;
                return false;
            }

            intersect = Create(left, top, right, bottom);
            return true;
        }
        #endregion

        #region 运算符重载
        public static bool operator ==(in AABB2D lhs, in AABB2D rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.W == rhs.W && lhs.H == rhs.H;
        public static bool operator !=(in AABB2D lhs, in AABB2D rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.W != rhs.W || lhs.H != rhs.H;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is AABB2D other && this == other;
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ W.GetHashCode() ^ H.GetHashCode();
        public bool Equals(AABB2D other) => this == other;
        public int CompareTo(AABB2D other)
        {
            int match0 = X.CompareTo(other.X);
            if (match0 != 0)
                return match0;

            int match1 = Y.CompareTo(other.Y);
            if (match1 != 0)
                return match1;

            int match2 = W.CompareTo(other.W);
            if (match2 != 0)
                return match2;

            int match3 = H.CompareTo(other.H);
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