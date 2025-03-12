using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的四维向量
    /// </summary>
    [Serializable]
    public struct Vector4D : IEquatable<Vector4D>, IComparable<Vector4D>, IFormattable
    {
        #region 字段/初始化
        public static readonly Vector4D Zero = new(0, 0, 0, 0);
        public static readonly Vector4D One = new(1, 1, 1, 1);
        public static readonly Vector4D Infinitesimal = new(Fixed64.Infinitesimal, Fixed64.Infinitesimal, Fixed64.Infinitesimal, Fixed64.Infinitesimal);
        public static readonly Vector4D Infinity = new(Fixed64.Infinity, Fixed64.Infinity, Fixed64.Infinity, Fixed64.Infinity);

        public Fixed64 X;
        public Fixed64 Y;
        public Fixed64 Z;
        public Fixed64 W;

        public Vector4D(Fixed64 x)
        {
            X = x;
            Y = Fixed64.Zero;
            Z = Fixed64.Zero;
            W = Fixed64.Zero;
        }
        public Vector4D(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
            Z = Fixed64.Zero;
            W = Fixed64.Zero;
        }
        public Vector4D(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            X = x;
            Y = y;
            Z = z;
            W = Fixed64.Zero;
        }
        public Vector4D(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Fixed64 this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new IndexOutOfRangeException($"Invalid Vector4D index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Vector4D index:{index}!");
                }
            }
        }
        public void Set(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 模长的平方
        /// </summary>
        public readonly Fixed64 SqrMagnitude() => X.Sqr() + Y.Sqr() + Z.Sqr() + W.Sqr();
        /// <summary>
        /// 模长
        /// </summary>
        public readonly Fixed64 Magnitude() => SqrMagnitude().Sqrt();
        /// <summary>
        /// 返回两点之间的距离的平方
        /// </summary>
        public static Fixed64 SqrDistance(in Vector4D lhs, in Vector4D rhs) => (lhs - rhs).SqrMagnitude();
        /// <summary>
        /// 返回两点之间的距离
        /// </summary>
        public static Fixed64 Distance(in Vector4D lhs, in Vector4D rhs) => (lhs - rhs).SqrMagnitude().Sqrt();

        /// <summary>
        /// 返回副本，其大小被限制为输入值
        /// </summary>
        public readonly Vector4D ClampMagnitude(Fixed64 maxDelta)
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
        public readonly Vector4D Normalized() => this * Magnitude().Reciprocal();
        /// <summary>
        /// 使该向量的模长为1
        /// </summary>
        public void Normalize() => this = Normalized();

        /// <summary>
        /// 点乘
        /// </summary>
        public static Fixed64 Dot(in Vector4D lhs, in Vector4D rhs) => lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z + lhs.W * rhs.W;
        /// <summary>
        /// 将两个向量的分量相乘
        /// </summary>
        public static Vector4D Scale(in Vector4D lhs, in Vector4D rhs) => new()
        {
            X = lhs.X * rhs.X,
            Y = lhs.Y * rhs.Y,
            Z = lhs.Z * rhs.Z,
            W = lhs.W * rhs.W,
        };

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Vector4D Abs() => new(X.Abs(), Y.Abs(), Z.Abs(), W.Abs());

        /// <summary>
        /// 区间值更正
        /// </summary>
        public readonly Vector4D Clamp(in Vector4D min, Vector4D max) => new()
        {
            X = X.Clamp(min.X, max.X),
            Y = Y.Clamp(min.Y, max.Y),
            Z = Z.Clamp(min.Z, max.Z),
            W = W.Clamp(min.W, max.W),
        };
        /// <summary>
        /// 较小值
        /// </summary>
        public static Vector4D Min(in Vector4D lsh, in Vector4D rsh) => new()
        {
            X = Fixed64.Min(lsh.X, rsh.X),
            Y = Fixed64.Min(lsh.Y, rsh.Y),
            Z = Fixed64.Min(lsh.Z, rsh.Z),
            W = Fixed64.Min(lsh.W, rsh.W),
        };
        /// <summary>
        /// 较大值
        /// </summary>
        public static Vector4D Max(in Vector4D lsh, in Vector4D rsh) => new()
        {
            X = Fixed64.Max(lsh.X, rsh.X),
            Y = Fixed64.Max(lsh.Y, rsh.Y),
            Z = Fixed64.Max(lsh.Z, rsh.Z),
            W = Fixed64.Max(lsh.W, rsh.W),
        };
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_STANDALONE
        public static implicit operator Vector4D(in UnityEngine.Vector4 value) => new(value.x, value.y, value.z, value.w);
        public static explicit operator UnityEngine.Vector4(in Vector4D value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
#endif

        public static implicit operator Vector4D(in System.Numerics.Vector4 value) => new(value.X, value.Y, value.Z, value.W);
        public static explicit operator System.Numerics.Vector4(in Vector4D value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);

        public static implicit operator Vector4D(in Vector2D value) => new(value.X, value.Y);
        public static explicit operator Vector2D(in Vector4D value) => new(value.X, value.Y);

        public static implicit operator Vector4D(in Vector3D value) => new(value.X, value.Y, value.Z);
        public static explicit operator Vector3D(in Vector4D value) => new(value.X, value.Y, value.Z);

        public static Vector4D operator +(in Vector4D value) => value;
        public static Vector4D operator -(in Vector4D value) => new(-value.X, -value.Y, -value.Z, -value.W);
        public static Vector4D operator +(in Vector4D lhs, in Vector4D rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
        public static Vector4D operator -(in Vector4D lhs, in Vector4D rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);

        public static Vector4D operator *(in Vector4D lhs, Fixed64 rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        public static Vector4D operator *(in Vector4D lhs, long rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        public static Vector4D operator *(Fixed64 lhs, in Vector4D rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
        public static Vector4D operator *(long lhs, in Vector4D rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
        public static Vector4D operator /(in Vector4D lhs, Fixed64 rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
        public static Vector4D operator /(in Vector4D lhs, long rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);

        public static bool operator ==(in Vector4D lhs, in Vector4D rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z && lhs.W == rhs.W;
        public static bool operator !=(in Vector4D lhs, in Vector4D rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z || lhs.W != rhs.W;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Vector4D other && this == other;
        public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        public readonly bool Equals(Vector4D other) => this == other;
        public readonly int CompareTo(Vector4D other)
        {
            int match0 = X.RawValue.CompareTo(other.X.RawValue);
            if (match0 != 0)
                return match0;

            int match1 = Y.RawValue.CompareTo(other.Y.RawValue);
            if (match1 != 0)
                return match1;

            int match2 = Z.RawValue.CompareTo(other.Z.RawValue);
            if (match2 != 0)
                return match2;

            int match3 = W.RawValue.CompareTo(other.W.RawValue);
            if (match3 != 0)
                return match3;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"({X.ToString(format, provider)}, {Y.ToString(format, provider)}, {Z.ToString(format, provider)}, {W.ToString(format, provider)})";
        #endregion
    }
}