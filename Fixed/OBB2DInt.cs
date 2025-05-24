using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 二维定向包围盒
    /// </summary>
    public readonly struct OBB2DInt : IEquatable<OBB2DInt>, IComparable<OBB2DInt>, IFormattable
    {
        #region 字段/构造方法
        public readonly int X; // 中心点（X）
        public readonly int Y; // 中心点（Y）
        public readonly int W; // 半宽
        public readonly int H; // 半高
        public readonly Angle A; // 角度

        public OBB2DInt(int x, int y, int e, Fixed64 a)
        {
            Check.Extents(e, nameof(e));

            X = x;
            Y = y;
            W = e;
            H = e;
            A = a;
        }
        public OBB2DInt(int x, int y, int w, int h, Fixed64 a)
        {
            Check.Extents(w, nameof(w));
            Check.Extents(h, nameof(h));

            X = x;
            Y = y;
            W = w;
            H = h;
            A = a;
        }
        public OBB2DInt(int x, int y, Vector2DInt e, Fixed64 a)
        {
            Check.Extents(e.X, "e.x");
            Check.Extents(e.Y, "e.y");

            X = x;
            Y = y;
            W = e.X;
            H = e.Y;
            A = a;
        }
        public OBB2DInt(Vector2DInt center, int extents, Fixed64 angle)
        {
            Check.Extents(extents, nameof(extents));

            X = center.X;
            Y = center.Y;
            W = extents;
            H = extents;
            A = angle;
        }
        public OBB2DInt(Vector2DInt center, int width, int height, Fixed64 angle)
        {
            Check.Extents(width, nameof(width));
            Check.Extents(height, nameof(height));

            X = center.X;
            Y = center.Y;
            W = width;
            H = height;
            A = angle;
        }
        public OBB2DInt(Vector2DInt center, Vector2DInt extents, Fixed64 angle)
        {
            Check.Extents(extents.X, "extents.x");
            Check.Extents(extents.Y, "extents.y");

            X = center.X;
            Y = center.Y;
            W = extents.X;
            H = extents.Y;
            A = angle;
        }
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
        #endregion

        #region 旋转后的尺寸/边界
        public Fixed64 RotatedLeft() => X + Geometry.RotateXDeg(ForLeftBoundary(), A); // 左
        public Fixed64 RotatedRight() => X + Geometry.RotateXDeg(ForRightBoundary(), A); // 右
        public Fixed64 RotatedBottom() => Y + Geometry.RotateYDeg(ForBottomBoundary(), A); // 下
        public Fixed64 RotatedTop() => Y + Geometry.RotateYDeg(ForTopBoundary(), A); // 上

        public Vector2D RotatedSize() => RotatedHalfSize() * 2; // 旋转后的尺寸
        public Vector2D RotatedHalfSize() // 旋转后的一半尺寸
        {
            var frb = ForRightBoundary();
            var ftb = ForTopBoundary();
            var dir = A.Direction();
            var rx = Geometry.RotateX(frb, in dir);
            var ry = Geometry.RotateY(ftb, in dir);
            return new Vector2D(rx, ry);
        }
        public Vector2D RotatedMin() // 旋转后的最小值
        {
            var size = RotatedHalfSize();
            return new Vector2D(X - size.X, Y - size.Y);
        }
        public Vector2D RotatedMax() // 旋转后的最大值
        {
            var size = RotatedHalfSize();
            return new Vector2D(X + size.X, Y + size.Y);
        }
        public void RotatedCorner(out Vector2D p0, out Vector2D p1, out Vector2D p2, out Vector2D p3) // 旋转后的4个角
        {
            var dir = A.Direction();
            var wx = W * dir.X;
            var wy = W * dir.Y;
            var hx = H * dir.X;
            var hy = H * dir.Y;
            p0 = new Vector2D(X - wx - hy, Y - wy + hx);
            p1 = new Vector2D(X + wx - hy, Y + wy + hx);
            p2 = new Vector2D(X - wx + hy, Y - wy - hx);
            p3 = new Vector2D(X + wx + hy, Y + wy - hx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Vector2DInt ForLeftBoundary() => A.Value.RawValue switch // 决定左边界的点
        {
            >= Const.Zero and < Const.Deg90 => new Vector2DInt(-W, H),
            >= Const.Deg90 and < Const.Deg180 => new Vector2DInt(W, H),
            >= Const.Deg180 and < Const.Deg270 => new Vector2DInt(W, -H),
            >= Const.Deg270 and < Const.Deg360 => new Vector2DInt(-W, -H),
            _ => throw new ArgumentOutOfRangeException(nameof(A), $"Invalid angle:{A}!"),
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Vector2DInt ForRightBoundary() => A.Value.RawValue switch // 决定右边界的点
        {
            >= Const.Zero and < Const.Deg90 => new Vector2DInt(W, -H),
            >= Const.Deg90 and < Const.Deg180 => new Vector2DInt(-W, -H),
            >= Const.Deg180 and < Const.Deg270 => new Vector2DInt(-W, H),
            >= Const.Deg270 and < Const.Deg360 => new Vector2DInt(W, H),
            _ => throw new ArgumentOutOfRangeException(nameof(A), $"Invalid angle:{A}!"),
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Vector2DInt ForBottomBoundary() => A.Value.RawValue switch // 决定下边界的点
        {
            >= Const.Zero and < Const.Deg90 => new Vector2DInt(-W, -H),
            >= Const.Deg90 and < Const.Deg180 => new Vector2DInt(-W, H),
            >= Const.Deg180 and < Const.Deg270 => new Vector2DInt(W, H),
            >= Const.Deg270 and < Const.Deg360 => new Vector2DInt(W, -H),
            _ => throw new ArgumentOutOfRangeException(nameof(A), $"Invalid angle:{A}!"),
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Vector2DInt ForTopBoundary() => A.Value.RawValue switch // 决定上边界的点
        {
            >= Const.Zero and < Const.Deg90 => new Vector2DInt(W, H),
            >= Const.Deg90 and < Const.Deg180 => new Vector2DInt(W, -H),
            >= Const.Deg180 and < Const.Deg270 => new Vector2DInt(-W, -H),
            >= Const.Deg270 and < Const.Deg360 => new Vector2DInt(-W, H),
            _ => throw new ArgumentOutOfRangeException(nameof(A), $"Invalid angle:{A}!"),
        };
        #endregion

        #region 基础方法
        //public bool Contain(Vector2DInt other) => Left() <= other.X && Right() >= other.X && Bottom() <= other.Y && Top() >= other.Y;
        //public bool Contain(in CircleInt other) => Left() <= other.X && Right() >= other.X && Bottom() <= other.Y && Top() >= other.Y;
        //public bool Contain(in AABB2DInt other) => Left() <= other.Left() && Right() >= other.Right() && Bottom() <= other.Bottom() && Top() >= other.Top(); // 是否包含aabb
        //public bool Contain(in OBB2DInt other) => Left() <= other.Left() && Right() >= other.Right() && Bottom() <= other.Bottom() && Top() >= other.Top(); // 是否包含aabb

        //public bool Intersect(in CircleInt other) // 检测aabb与圆是否相交
        //{
        //    int x = Math.Max(Math.Abs(X - other.X) - W, 0);
        //    int y = Math.Max(Math.Abs(Y - other.Y) - H, 0);
        //    return x * x + y * y <= other.R * other.R;
        //}
        //public bool Intersect_Box_Circle(in AABB2DInt circle) => IntersectBoxAndCircle(in this, in circle); // 检测aabb与圆是否相交
        //public bool Intersect_Circle_Box(in AABB2DInt box) => IntersectBoxAndCircle(in box, in this); // 检测圆与aabb是否相交
        //public bool Intersect_Circle_Circle(in AABB2DInt other) // 检测圆与圆是否相交
        //{
        //    int x = X - other.X;
        //    int y = Y - other.Y;
        //    int w = W + other.W;
        //    int h = H + other.H;
        //    return x * x + y * y <= w * h;
        //}
        //public bool Intersect_Box_Box(in AABB2DInt other) => Math.Abs(X - other.X) < W + other.W && Math.Abs(Y - other.Y) < H + other.H; // 检测aabb与aabb是否相交

        //private bool IntersectBoxAndCircle(in AABB2DInt box, in AABB2DInt circle)
        //{
        //    ulong x = (ulong)Math.Max(Math.Abs(box.X - circle.X) - box.W, 0);
        //    ulong y = (ulong)Math.Max(Math.Abs(box.Y - circle.Y) - box.H, 0);
        //    return x * x + y * y <= (ulong)circle.W * (ulong)circle.H;
        //}
        #endregion

        #region 运算符重载
        public static bool operator ==(in OBB2DInt lhs, in OBB2DInt rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.W == rhs.W && lhs.H == rhs.H && lhs.A == rhs.A;
        public static bool operator !=(in OBB2DInt lhs, in OBB2DInt rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.W != rhs.W || lhs.H != rhs.H || lhs.A != rhs.A;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is OBB2DInt other && this == other;
        public override int GetHashCode() => X ^ Y ^ W ^ H;
        public bool Equals(OBB2DInt other) => this == other;
        public int CompareTo(OBB2DInt other)
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

            int match4 = A.CompareTo(other.A);
            if (match4 != 0)
                return match4;

            return 0;
        }

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider) => $"Center:{Center().ToString(format, provider)}, Size:{Size().ToString(format, provider)}, Angle:{A.ToString(format, provider)}";
        #endregion
    }
}