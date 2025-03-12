using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的二维向量
    /// </summary>
    [Serializable]
    public struct Vector2D : IEquatable<Vector2D>, IComparable<Vector2D>, IFormattable
    {
        #region 字段/初始化
        public static readonly Vector2D Zero = new(0, 0);
        public static readonly Vector2D One = new(1, 1);
        public static readonly Vector2D Right = new(1, 0);
        public static readonly Vector2D Left = new(-1, 0);
        public static readonly Vector2D Up = new(0, 1);
        public static readonly Vector2D Down = new(0, -1);
        public static readonly Vector2D Infinitesimal = new(Fixed64.Infinitesimal, Fixed64.Infinitesimal);
        public static readonly Vector2D Infinity = new(Fixed64.Infinity, Fixed64.Infinity);

        public Fixed64 X;
        public Fixed64 Y;

        public Vector2D(Fixed64 x)
        {
            X = x;
            Y = Fixed64.Zero;
        }
        public Vector2D(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
        }

        public Fixed64 this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                _ => throw new IndexOutOfRangeException($"Invalid Vector2D index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Vector2D index:{index}!");
                }
            }
        }
        public void Set(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 模长的平方
        /// </summary>
        public readonly Fixed64 SqrMagnitude() => X.Sqr() + Y.Sqr();
        /// <summary>
        /// 模长
        /// </summary>
        public readonly Fixed64 Magnitude() => SqrMagnitude().Sqrt();
        /// <summary>
        /// 返回两点之间的距离的平方
        /// </summary>
        public static Fixed64 SqrDistance(in Vector2D lhs, in Vector2D rhs) => (lhs - rhs).SqrMagnitude();
        /// <summary>
        /// 返回两点之间的距离
        /// </summary>
        public static Fixed64 Distance(in Vector2D lhs, in Vector2D rhs) => (lhs - rhs).SqrMagnitude().Sqrt();

        /// <summary>
        /// 返回副本，其大小被限制为输入值
        /// </summary>
        public readonly Vector2D ClampMagnitude(Fixed64 maxDelta)
        {
            switch (maxDelta.RawValue)
            {
                case < 0: throw new ArgumentOutOfRangeException(nameof(maxDelta), $"{maxDelta} < 0");
                case 0: return Zero;
            }

            var sqrMagnitude = SqrMagnitude();
            if (sqrMagnitude.RawValue == 0)
                return Zero;

            var sqrMaxLength = maxDelta.Sqr();
            if (sqrMaxLength >= sqrMagnitude)
                return this;

            return this * (sqrMaxLength / sqrMagnitude).Sqrt();
        }
        /// <summary>
        /// 返回该向量的模长为1的向量
        /// </summary>
        public readonly Vector2D Normalized() => this * Magnitude().Reciprocal();
        /// <summary>
        /// 使该向量的模长为1
        /// </summary>
        public void Normalize() => this = Normalized();

        /// <summary>
        /// 点乘
        /// </summary>
        public static Fixed64 Dot(in Vector2D lhs, in Vector2D rhs) => lhs.X * rhs.X + lhs.Y * rhs.Y;
        /// <summary>
        /// 叉乘
        /// </summary>
        public static Fixed64 Cross(in Vector2D lhs, in Vector2D rhs) => lhs.X * rhs.Y - lhs.Y * rhs.X;
        /// <summary>
        /// 将两个向量的分量相乘
        /// </summary>
        public static Vector2D Scale(in Vector2D lhs, in Vector2D rhs) => new()
        {
            X = lhs.X * rhs.X,
            Y = lhs.Y * rhs.Y,
        };

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Vector2D Abs() => new(X.Abs(), Y.Abs());
        /// <summary>
        /// 返回垂直于该向量的向量，对于正Y轴向上的坐标系来说，结果始终沿逆时针方向旋转90度
        /// </summary>
        public readonly Vector2D Perpendicular() => new(-Y, X);
        /// <summary>
        /// 从法线定义的向量反射一个向量
        /// </summary>
        public static Vector2D Reflect(in Vector2D inDirection, in Vector2D inNormal)
        {
            var dot = Dot(in inDirection, in inNormal) << 1;
            return new Vector2D(inDirection.X - dot * inNormal.X, inDirection.Y - dot * inNormal.Y);
        }

        /// <summary>
        /// 区间值更正
        /// </summary>
        public readonly Vector2D Clamp(in Vector2D min, Vector2D max) => new()
        {
            X = X.Clamp(min.X, max.X),
            Y = Y.Clamp(min.Y, max.Y),
        };
        /// <summary>
        /// 较小值
        /// </summary>
        public static Vector2D Min(in Vector2D lsh, in Vector2D rsh) => new()
        {
            X = Fixed64.Min(lsh.X, rsh.X),
            Y = Fixed64.Min(lsh.Y, rsh.Y),
        };
        /// <summary>
        /// 较大值
        /// </summary>
        public static Vector2D Max(in Vector2D lsh, in Vector2D rsh) => new()
        {
            X = Fixed64.Max(lsh.X, rsh.X),
            Y = Fixed64.Max(lsh.Y, rsh.Y),
        };
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_STANDALONE
        public static implicit operator Vector2D(UnityEngine.Vector2 value) => new(value.x, value.y);
        public static explicit operator UnityEngine.Vector2(in Vector2D value) => new((float)value.X, (float)value.Y);

        public static implicit operator Vector2D(UnityEngine.Vector2Int value) => new(value.x, value.y);
        public static explicit operator UnityEngine.Vector2Int(in Vector2D value) => new((int)value.X, (int)value.Y);
#endif

        public static implicit operator Vector2D(System.Numerics.Vector2 value) => new(value.X, value.Y);
        public static explicit operator System.Numerics.Vector2(in Vector2D value) => new((float)value.X, (float)value.Y);

        public static Vector2D operator +(in Vector2D value) => value;
        public static Vector2D operator -(in Vector2D value) => new(-value.X, -value.Y);
        public static Vector2D operator +(in Vector2D lhs, in Vector2D rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static Vector2D operator -(in Vector2D lhs, in Vector2D rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static Vector2D operator *(in Vector2D lhs, Fixed64 rhs) => new(lhs.X * rhs, lhs.Y * rhs);
        public static Vector2D operator *(in Vector2D lhs, long rhs) => new(lhs.X * rhs, lhs.Y * rhs);
        public static Vector2D operator *(Fixed64 lhs, in Vector2D rhs) => new(lhs * rhs.X, lhs * rhs.Y);
        public static Vector2D operator *(long lhs, in Vector2D rhs) => new(lhs * rhs.X, lhs * rhs.Y);
        public static Vector2D operator /(in Vector2D lhs, Fixed64 rhs) => new(lhs.X / rhs, lhs.Y / rhs);
        public static Vector2D operator /(in Vector2D lhs, long rhs) => new(lhs.X / rhs, lhs.Y / rhs);

        public static bool operator ==(in Vector2D lhs, in Vector2D rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(in Vector2D lhs, in Vector2D rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Vector2D other && this == other;
        public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
        public readonly bool Equals(Vector2D other) => this == other;
        public readonly int CompareTo(Vector2D other)
        {
            int match0 = X.RawValue.CompareTo(other.X.RawValue);
            if (match0 != 0)
                return match0;

            int match1 = Y.RawValue.CompareTo(other.Y.RawValue);
            if (match1 != 0)
                return match1;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"({X.ToString(format, provider)}, {Y.ToString(format, provider)})";
        #endregion
    }
}