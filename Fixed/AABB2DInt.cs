using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 二维轴对齐包围盒
    /// </summary>
    public readonly struct AABB2DInt : IEquatable<AABB2DInt>, IComparable<AABB2DInt>, IFormattable
    {
        #region 字段/构造方法
        public readonly int X; // 中心点（X）
        public readonly int Y; // 中心点（Y）
        public readonly int W; // 半宽
        public readonly int H; // 半高

        public AABB2DInt(int x, int y, int e)
        {
            Check.Extents(e, nameof(e));

            X = x;
            Y = y;
            W = e;
            H = e;
        }
        public AABB2DInt(int x, int y, int w, int h)
        {
            Check.Extents(w, nameof(w));
            Check.Extents(h, nameof(h));

            X = x;
            Y = y;
            W = w;
            H = h;
        }
        internal AABB2DInt(int x, int y, int w, int h, bool _) // 不安全的构建，绕过检测
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
        public AABB2DInt(int x, int y, Vector2DInt e)
        {
            Check.Extents(e.X, "e.x");
            Check.Extents(e.Y, "e.y");

            X = x;
            Y = y;
            W = e.X;
            H = e.Y;
        }
        public AABB2DInt(Vector2DInt center, int extents)
        {
            Check.Extents(extents, nameof(extents));

            X = center.X;
            Y = center.Y;
            W = extents;
            H = extents;
        }
        public AABB2DInt(Vector2DInt center, int width, int height)
        {
            Check.Extents(width, nameof(width));
            Check.Extents(height, nameof(height));

            X = center.X;
            Y = center.Y;
            W = width;
            H = height;
        }
        public AABB2DInt(Vector2DInt center, Vector2DInt extents)
        {
            Check.Extents(extents.X, "extents.x");
            Check.Extents(extents.Y, "extents.y");

            X = center.X;
            Y = center.Y;
            W = extents.X;
            H = extents.Y;
        }

        public static AABB2DInt Create(int xMin, int xMax, int yMin, int yMax) => new(xMax + xMin >> 1, yMax + yMin >> 1, xMax - xMin >> 1, yMax - yMin >> 1);
        internal static AABB2DInt UnsafeCreate(int xMin, int xMax, int yMin, int yMax) => new(xMax + xMin >> 1, yMax + yMin >> 1, xMax - xMin >> 1, yMax - yMin >> 1, false);
        #endregion

        #region 中心点/尺寸/边界
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left() => X - W;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Right() => X + W;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Bottom() => Y - H;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Top() => Y + H;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Center() => new(X, Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Size() => new(W << 1, H << 1); // 尺寸
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt HalfSize() => new(W, H); // 一半尺寸
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Min() => LeftBottom();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Max() => RightTop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt LeftBottom() => new(Left(), Bottom());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt RightBottom() => new(Right(), Bottom());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt RightTop() => new(Right(), Top());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt LeftTop() => new(Left(), Top());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2DInt LeftBottomAABB() // 左下AABB，原先1/4的AABB
        {
            int w = W >> 1;
            int h = H >> 1;
            return new AABB2DInt(X - w, Y - h, w, h);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2DInt RightBottomAABB() // 右下AABB，原先1/4的AABB
        {
            int w = W >> 1;
            int h = H >> 1;
            return new AABB2DInt(X + w, Y - h, w, h);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2DInt RightTopAABB() // 右上AABB，原先1/4的AABB
        {
            int w = W >> 1;
            int h = H >> 1;
            return new AABB2DInt(X + w, Y + h, w, h);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AABB2DInt LeftTopAABB() // 左上AABB，原先1/4的AABB
        {
            int w = W >> 1;
            int h = H >> 1;
            return new AABB2DInt(X - w, Y + h, w, h);
        }
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static implicit operator AABB2D(in AABB2DInt value) => new(value.X, value.Y, value.W, value.H);
        public static explicit operator AABB2DInt(in AABB2D value) => new((int)value.X, (int)value.Y, (int)value.W, (int)value.H);

        public static bool operator ==(in AABB2DInt lhs, in AABB2DInt rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.W == rhs.W && lhs.H == rhs.H;
        public static bool operator !=(in AABB2DInt lhs, in AABB2DInt rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.W != rhs.W || lhs.H != rhs.H;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is AABB2DInt other && this == other;
        public override int GetHashCode() => X ^ Y ^ W ^ H;
        public bool Equals(AABB2DInt other) => this == other;
        public int CompareTo(AABB2DInt other)
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