using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的三维向量
    /// </summary>
    [Serializable]
    public struct Vector3D : IEquatable<Vector3D>, IComparable<Vector3D>, IFormattable
    {
        #region 字段/初始化
        public static readonly Vector3D Zero = new(0, 0, 0);
        public static readonly Vector3D One = new(1, 1, 1);
        public static readonly Vector3D Up = new(0, 1, 0);
        public static readonly Vector3D Down = new(0, -1, 0);
        public static readonly Vector3D Left = new(-1, 0, 0);
        public static readonly Vector3D Right = new(1, 0, 0);
        public static readonly Vector3D Forward = new(0, 0, 1);
        public static readonly Vector3D Back = new(0, 0, -1);
        public static readonly Vector3D Infinitesimal = new(Fixed64.Infinitesimal, Fixed64.Infinitesimal, Fixed64.Infinitesimal);
        public static readonly Vector3D Infinity = new(Fixed64.Infinity, Fixed64.Infinity, Fixed64.Infinity);

        public Fixed64 X;
        public Fixed64 Y;
        public Fixed64 Z;

        public Vector3D(Fixed64 x)
        {
            X = x;
            Y = default;
            Z = default;
        }
        public Vector3D(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
            Z = default;
        }
        public Vector3D(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Fixed64 this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new IndexOutOfRangeException($"Invalid Vector3D index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Vector3D index:{index}!");
                }
            }
        }
        public void Set(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 模长的平方
        /// </summary>
        public readonly Fixed64 SqrMagnitude() => X.Sqr() + Y.Sqr() + Z.Sqr();
        /// <summary>
        /// 模长
        /// </summary>
        public readonly Fixed64 Magnitude() => SqrMagnitude().Sqrt();
        /// <summary>
        /// 返回两点之间的距离的平方
        /// </summary>
        public static Fixed64 SqrDistance(in Vector3D lhs, in Vector3D rhs) => (lhs - rhs).SqrMagnitude();
        /// <summary>
        /// 返回两点之间的距离
        /// </summary>
        public static Fixed64 Distance(in Vector3D lhs, in Vector3D rhs) => (lhs - rhs).SqrMagnitude().Sqrt();

        /// <summary>
        /// 返回副本，缩放模长
        /// </summary>
        public readonly Vector3D ScaleMagnitude(Fixed64 scale)
        {
            if (scale.RawValue == 0)
                return Zero;

            var sqrMagnitude = SqrMagnitude();
            if (sqrMagnitude.RawValue == 0)
                return Zero;

            return this * (scale / sqrMagnitude.Sqrt());
        }
        /// <summary>
        /// 返回副本，其大小被限制为输入值
        /// </summary>
        public readonly Vector3D ClampMagnitude(Fixed64 maxDelta)
        {
            if (maxDelta.RawValue == 0)
                return Zero;

            var sqrMagnitude = SqrMagnitude();
            if (sqrMagnitude.RawValue == 0)
                return Zero;

            if (sqrMagnitude <= maxDelta.Sqr())
                return this;

            return this * (maxDelta / sqrMagnitude.Sqrt());
        }
        /// <summary>
        /// 返回该向量的模长为1的向量
        /// </summary>
        public readonly Vector3D Normalized() => this / Magnitude();
        /// <summary>
        /// 使该向量的模长为1
        /// </summary>
        public void Normalize() => this = Normalized();

        /// <summary>
        /// 点乘
        /// </summary>
        public static Fixed64 Dot(in Vector3D lhs, in Vector3D rhs) => lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
        /// <summary>
        /// 叉乘
        /// </summary>
        public static Vector3D Cross(in Vector3D lhs, in Vector3D rhs) => new()
        {
            X = lhs.Y * rhs.Z - lhs.Z * rhs.Y,
            Y = lhs.Z * rhs.X - lhs.X * rhs.Z,
            Z = lhs.X * rhs.Y - lhs.Y * rhs.X,
        };
        /// <summary>
        /// 将两个向量的分量相乘
        /// </summary>
        public static Vector3D Scale(in Vector3D lhs, in Vector3D rhs) => new()
        {
            X = lhs.X * rhs.X,
            Y = lhs.Y * rhs.Y,
            Z = lhs.Z * rhs.Z,
        };

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Vector3D Abs() => new(X.Abs(), Y.Abs(), Z.Abs());
        /// <summary>
        /// 从法线定义的向量反射一个向量
        /// </summary>
        public static Vector3D Reflect(in Vector3D inDirection, in Vector3D inNormal)
        {
            var dot = Dot(in inDirection, in inNormal) << 1;
            return inDirection - inNormal * dot;
        }
        /// <summary>
        /// 将向量投影到另一个向量上
        /// </summary>
        public static Vector3D Project(in Vector3D direction, in Vector3D onNormal)
        {
            var sqrMagnitude = onNormal.SqrMagnitude();
            if (sqrMagnitude.RawValue <= Const.Epsilon)
                return Zero;

            var dot = Dot(in direction, in onNormal);
            return dot / sqrMagnitude * onNormal;
        }
        /// <summary>
        /// 将向量投影到由法线定义的平面上（法线与该平面正交）
        /// </summary>
        public static Vector3D ProjectOnPlane(in Vector3D direction, in Vector3D planeNormal)
        {
            var sqrMagnitude = planeNormal.SqrMagnitude();
            if (sqrMagnitude.RawValue <= Const.Epsilon)
                return Zero;

            var project = Project(in direction, in planeNormal);
            return direction - project;
        }

        /// <summary>
        /// 区间值更正
        /// </summary>
        public readonly Vector3D Clamp(in Vector3D min, Vector3D max) => new()
        {
            X = X.Clamp(min.X, max.X),
            Y = Y.Clamp(min.Y, max.Y),
            Z = Z.Clamp(min.Z, max.Z),
        };
        /// <summary>
        /// 较小值
        /// </summary>
        public static Vector3D Min(in Vector3D lsh, in Vector3D rsh) => new()
        {
            X = Fixed64.Min(lsh.X, rsh.X),
            Y = Fixed64.Min(lsh.Y, rsh.Y),
            Z = Fixed64.Min(lsh.Z, rsh.Z),
        };
        /// <summary>
        /// 较大值
        /// </summary>
        public static Vector3D Max(in Vector3D lsh, in Vector3D rsh) => new()
        {
            X = Fixed64.Max(lsh.X, rsh.X),
            Y = Fixed64.Max(lsh.Y, rsh.Y),
            Z = Fixed64.Max(lsh.Z, rsh.Z),
        };
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_5_3_OR_NEWER
        public static implicit operator Vector3D(in UnityEngine.Vector3 value) => new(value.x, value.y, value.z);
        public static explicit operator UnityEngine.Vector3(in Vector3D value) => new((float)value.X, (float)value.Y, (float)value.Z);

        public static implicit operator Vector3D(in UnityEngine.Vector3Int value) => new(value.x, value.y, value.z);
        public static explicit operator UnityEngine.Vector3Int(in Vector3D value) => new((int)value.X, (int)value.Y, (int)value.Z);
#endif

        public static implicit operator Vector3D(in System.Numerics.Vector3 value) => new(value.X, value.Y, value.Z);
        public static explicit operator System.Numerics.Vector3(in Vector3D value) => new((float)value.X, (float)value.Y, (float)value.Z);

        public static implicit operator Vector3D(in Vector2D value) => new(value.X, value.Y);
        public static explicit operator Vector2D(in Vector3D value) => new(value.X, value.Y);

        public static Vector3D operator +(in Vector3D value) => value;
        public static Vector3D operator -(in Vector3D value) => new(-value.X, -value.Y, -value.Z);
        public static Vector3D operator +(in Vector3D lhs, in Vector3D rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        public static Vector3D operator -(in Vector3D lhs, in Vector3D rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);

        public static Vector3D operator *(in Vector3D lhs, Fixed64 rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        public static Vector3D operator *(in Vector3D lhs, long rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        public static Vector3D operator *(Fixed64 lhs, in Vector3D rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z);
        public static Vector3D operator *(long lhs, in Vector3D rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z);
        public static Vector3D operator /(in Vector3D lhs, Fixed64 rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs); // “Fixed64.Reciprocal()”在大数运算下，会丢失精度
        public static Vector3D operator /(in Vector3D lhs, long rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);

        public static bool operator ==(in Vector3D lhs, in Vector3D rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
        public static bool operator !=(in Vector3D lhs, in Vector3D rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Vector3D other && this == other;
        public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        public readonly bool Equals(Vector3D other) => this == other;
        public readonly int CompareTo(Vector3D other)
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

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"({X.ToString(format, provider)}, {Y.ToString(format, provider)}, {Z.ToString(format, provider)})";
        #endregion
    }
}