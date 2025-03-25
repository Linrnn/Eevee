using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的矩形
    /// </summary>
    [Serializable]
    public struct Rectangle : IEquatable<Rectangle>, IComparable<Rectangle>, IFormattable
    {
        #region 字段/初始化
        public static readonly Rectangle Zero = new(0, 0, 0, 0);

        public Fixed64 X; // 等价于XMin
        public Fixed64 Y; // 等价于YMin
        public Fixed64 Width;
        public Fixed64 Height;

        public Rectangle(Fixed64 xMin, Fixed64 yMin, Fixed64 width, Fixed64 height)
        {
            X = xMin;
            Y = yMin;
            Width = width;
            Height = height;
        }
        public Rectangle(in Vector2D position, in Vector2D size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public void Set(Fixed64 xMin, Fixed64 yMin, Fixed64 width, Fixed64 height)
        {
            X = xMin;
            Y = yMin;
            Width = width;
            Height = height;
        }
        public void Set(in Vector2D position, in Vector2D size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }
        #endregion

        #region 基础方法
        public Vector2D Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(X, Y);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public Vector2D Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(X + (Width >> 1), Y + (Height >> 1));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                X = value.X - (Width >> 1);
                Y = value.Y - (Height >> 1);
            }
        }
        public Vector2D Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(Width, Height);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Vector2D Min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(X, Y);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                XMin = value.X;
                YMin = value.Y;
            }
        }
        public Fixed64 XMin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Width = XMax - value;
                X = value; // “X”会影响“XMax”的值，所以要后修改“X”
            }
        }
        public Fixed64 YMin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Height = YMax - value;
                Y = value; // “Y”会影响“YMax”的值，所以要后修改“Y”
            }
        }

        public Vector2D Max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(XMax, YMax);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                XMax = value.X;
                YMax = value.Y;
            }
        }
        public Fixed64 XMax
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Width + X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Width = value - X;
        }
        public Fixed64 YMax
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Height + Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Height = value - Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle MinMaxRect(Fixed64 xMin, Fixed64 yMin, Fixed64 xMax, Fixed64 yMax) => new(xMin, yMin, xMax - xMin, yMax - yMin);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Rectangle OrderMinMax()
        {
            var max = Max;
            var value = this;
            if (X > max.X)
                (value.X, value.XMax) = (max.X, X);
            if (Y > max.Y)
                (value.Y, value.YMax) = (max.Y, Y);
            return value;
        }

        /// <summary>
        /// 矩形重叠
        /// </summary>
        /// <param name="other">另一个矩形</param>
        /// <param name="inverse">允许宽度为负数</param>
        public readonly bool Overlaps(in Rectangle other, bool inverse) => inverse ? OrderMinMax().Overlaps(other.OrderMinMax()) : Overlaps(in other);
        /// <summary>
        /// 矩形重叠
        /// </summary>
        public readonly bool Overlaps(in Rectangle other) => X < other.XMax && XMax > other.X && Y < other.YMax && YMax > other.Y;
        /// <summary>
        /// 矩形重叠区域
        /// </summary>
        /// <param name="other">另一个矩形</param>
        /// <param name="inverse">允许宽度为负数</param>
        /// <param name="area">重叠区域</param>
        public readonly bool OverlapsArea(in Rectangle other, bool inverse, out Rectangle area) => inverse ? OrderMinMax().OverlapsArea(other.OrderMinMax(), out area) : OverlapsArea(in other, out area);
        /// <summary>
        /// 矩形重叠区域
        /// </summary>
        public readonly bool OverlapsArea(in Rectangle other, out Rectangle area)
        {
            var xMin = Fixed64.Max(X, other.X);
            var xMax = Fixed64.Min(XMax, other.XMax);
            if (xMin >= xMax)
            {
                area = Zero;
                return false;
            }

            var yMin = Fixed64.Max(Y, other.Y);
            var yMax = Fixed64.Min(YMax, other.YMax);
            if (yMin >= yMax)
            {
                area = Zero;
                return false;
            }

            area = MinMaxRect(xMin, yMin, xMax, yMax);
            return true;
        }

        /// <summary>
        /// 矩形包含点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in Vector2D point) => Contains(point.X, point.Y);
        /// <summary>
        /// 矩形包含点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsXY(in Vector3D point) => Contains(point.X, point.Y);
        /// <summary>
        /// 矩形包含点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsXZ(in Vector3D point) => Contains(point.X, point.Z);
        /// <summary>
        /// 矩形包含点
        /// </summary>
        /// <param name="x">X轴坐标</param>
        /// <param name="y">Y轴坐标</param>
        /// <param name="inverse">允许宽度为负数</param>
        public readonly bool Contains(Fixed64 x, Fixed64 y, bool inverse)
        {
            if (!inverse)
                return Contains(x, y);

            var xMax = XMax;
            if ((Width.RawValue >= 0 || x > X || x <= xMax) && (Width.RawValue < 0 || x < X || x >= xMax))
                return false;

            var yMax = YMax;
            if ((Height.RawValue >= 0 || y > Y || y <= yMax) && (Height.RawValue < 0 || y < Y || y >= yMax))
                return false;

            return true;
        }
        /// <summary>
        /// 矩形包含点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Fixed64 x, Fixed64 y) => x >= X && x < XMax && y >= Y && y < YMax;
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_5_3_OR_NEWER
        public static implicit operator Rectangle(in UnityEngine.Rect value) => new(value.xMin, value.yMin, value.width, value.height);
        public static explicit operator UnityEngine.Rect(in Rectangle value) => new((float)value.X, (float)value.Y, (float)value.Width, (float)value.Height);

        public static implicit operator Rectangle(in UnityEngine.RectInt value) => new(value.xMin, value.yMin, value.width, value.height);
        public static explicit operator UnityEngine.RectInt(in Rectangle value) => new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
#endif

        public static bool operator ==(in Rectangle lhs, in Rectangle rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Width == rhs.Width && lhs.Height == rhs.Height;
        public static bool operator !=(in Rectangle lhs, in Rectangle rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Width != rhs.Width || lhs.Height != rhs.Height;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Rectangle other && this == other;
        public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        public readonly bool Equals(Rectangle other) => this == other;
        public readonly int CompareTo(Rectangle other)
        {
            int match0 = X.RawValue.CompareTo(other.X.RawValue);
            if (match0 != 0)
                return match0;

            int match1 = Y.RawValue.CompareTo(other.Y.RawValue);
            if (match1 != 0)
                return match1;

            int match2 = Width.RawValue.CompareTo(other.Width.RawValue);
            if (match2 != 0)
                return match2;

            int match3 = Height.RawValue.CompareTo(other.Height.RawValue);
            if (match3 != 0)
                return match3;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"(x:{X.ToString(format, provider)}, y:{Y.ToString(format, provider)}, w:{Width.ToString(format, provider)}, h:{Height.ToString(format, provider)})";
        #endregion
    }
}