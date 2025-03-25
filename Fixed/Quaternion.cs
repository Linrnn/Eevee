using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的四元数
    /// </summary>
    [Serializable]
    public struct Quaternion : IEquatable<Quaternion>, IComparable<Quaternion>, IFormattable
    {
        #region 字段/初始化
        public static readonly Quaternion Identity = new(0, 0, 0, 1);

        public Fixed64 X;
        public Fixed64 Y;
        public Fixed64 Z;
        public Fixed64 W;

        public Quaternion(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Quaternion(in Vector3D xyz, Fixed64 w)
        {
            X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
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
                _ => throw new IndexOutOfRangeException($"Invalid Quaternion index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Quaternion index:{index}!");
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
        public void Set(in Vector3D xyz, Fixed64 w)
        {
            X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
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
        /// 返回该四元数的模长为1的向量
        /// </summary>
        public readonly Quaternion Normalized() => this / Magnitude();
        /// <summary>
        /// 使该四元数的模长为1
        /// </summary>
        public void Normalize() => this = Normalized();

        /// <summary>
        /// 两个四元数之间的点积
        /// </summary>
        public static Fixed64 Dot(in Quaternion lhs, in Quaternion rhs) => lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z + lhs.W * rhs.W;

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Quaternion Abs() => new(X.Abs(), Y.Abs(), Z.Abs(), W.Abs());
        /// <summary>
        /// 共轭
        /// </summary>
        public readonly Quaternion Conjugate() => new(-X, -Y, -Z, W);
        /// <summary>
        /// 反转
        /// </summary>
        public readonly Quaternion Inverse() => Conjugate() / SqrMagnitude();

        public static Quaternion FromToRotation(in Vector3D from, in Vector3D to)
        {
            var cross = Vector3D.Cross(in from, in to);
            var dot = Vector3D.Dot(in from, in to);
            var sqrt = (from.SqrMagnitude() * to.SqrMagnitude()).Sqrt();
            var quaternion = new Quaternion(cross.X, cross.Y, cross.Z, dot + sqrt);
            return quaternion.Normalized();
        }
        public void SetFromToRotation(in Vector3D fromDirection, in Vector3D toDirection) => this = FromToRotation(in fromDirection, in toDirection);
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_5_3_OR_NEWER
        public static implicit operator Quaternion(in UnityEngine.Quaternion value) => new(value.x, value.y, value.z, value.w);
        public static explicit operator UnityEngine.Quaternion(in Quaternion value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
#endif

        public static implicit operator Quaternion(in System.Numerics.Quaternion value) => new(value.X, value.Y, value.Z, value.W);
        public static explicit operator System.Numerics.Quaternion(in Quaternion value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);

        public static Quaternion operator +(in Quaternion value) => value;
        public static Quaternion operator -(in Quaternion value) => new(-value.X, -value.Y, -value.Z, -value.W);
        public static Quaternion operator +(in Quaternion lhs, in Quaternion rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
        public static Quaternion operator -(in Quaternion lhs, in Quaternion rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);

        public static Vector3D operator *(in Vector3D lhs, in Quaternion rhs) => rhs * lhs;
        public static Vector3D operator *(in Quaternion lhs, in Vector3D rhs)
        {
            var x2 = lhs.X << 1;
            var y2 = lhs.Y << 1;
            var z2 = lhs.Z << 1;
            var xx = lhs.X * x2;
            var yy = lhs.Y * y2;
            var zz = lhs.Z * z2;
            var xy = lhs.X * y2;
            var xz = lhs.X * z2;
            var yz = lhs.Y * z2;
            var wx = lhs.W * x2;
            var wy = lhs.W * y2;
            var wz = lhs.W * z2;
            return new Vector3D
            {
                X = (Fixed64.One - yy - zz) * rhs.X + (xy - wz) * rhs.Y + (xz + wy) * rhs.Z,
                Y = (xy + wz) * rhs.X + (Fixed64.One - xx - zz) * rhs.Y + (yz - wx) * rhs.Z,
                Z = (xz - wy) * rhs.X + (yz + wx) * rhs.Y + (Fixed64.One - xx - yy) * rhs.Z,
            };
        }
        public static Quaternion operator *(in Quaternion lhs, in Quaternion rhs) => new()
        {
            X = lhs.X * rhs.W + lhs.W * rhs.X + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
            Y = lhs.Y * rhs.W + lhs.W * rhs.Y + lhs.Z * rhs.X - lhs.X * rhs.Z,
            Z = lhs.Z * rhs.W + lhs.W * rhs.Z + lhs.X * rhs.Y - lhs.Y * rhs.X,
            W = lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z,
        };
        public static Quaternion operator *(in Quaternion lhs, Fixed64 rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        public static Quaternion operator *(in Quaternion lhs, long rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        public static Quaternion operator *(Fixed64 lhs, in Quaternion rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
        public static Quaternion operator *(long lhs, in Quaternion rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
        public static Quaternion operator /(in Quaternion lhs, Fixed64 rhs)
        {
            var reciprocal = rhs.Reciprocal();
            return new Quaternion(lhs.X * reciprocal, lhs.Y * reciprocal, lhs.Z * reciprocal, lhs.W * reciprocal);
        }
        public static Quaternion operator /(in Quaternion lhs, long rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);

        public static bool operator ==(in Quaternion lhs, in Quaternion rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z && lhs.W == rhs.W;
        public static bool operator !=(in Quaternion lhs, in Quaternion rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z || lhs.W != rhs.W;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Quaternion other && this == other;
        public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        public readonly bool Equals(Quaternion other) => this == other;
        public readonly int CompareTo(Quaternion other)
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