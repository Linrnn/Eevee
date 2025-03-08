using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的四元数
    /// </summary>
    [Serializable]
    public struct Quaternions : IEquatable<Quaternions>, IComparable<Quaternions>, IFormattable
    {
        #region 字段/初始化
        public static readonly Quaternions Identity = new(0, 0, 0, 1);

        public Fixed64 X;
        public Fixed64 Y;
        public Fixed64 Z;
        public Fixed64 W;

        public Quaternions(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Quaternions(in Vector3D xyz, Fixed64 w)
        {
            X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
            W = w;
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
        /// 返回该向量的模长为1的向量
        /// </summary>
        public readonly Quaternions Normalized() => this * Magnitude().Reciprocal();
        /// <summary>
        /// 使该向量的模长为1
        /// </summary>
        public void Normalize() => this = Normalized();

        /// <summary>
        /// 两个旋转之间的点积
        /// </summary>
        public static Fixed64 Dot(in Quaternions lhs, in Quaternions rhs) => lhs.W * rhs.W + lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Quaternions Abs() => new(X.Abs(), Y.Abs(), Z.Abs(), W.Abs());
        /// <summary>
        /// 共轭
        /// </summary>
        public readonly Quaternions Conjugate() => new(-X, -Y, -Z, W);
        /// <summary>
        /// 反转
        /// </summary>
        public readonly Quaternions Inverse() => Conjugate() / SqrMagnitude();

        public static Quaternions FromToRotation(in Vector3D fromDirection, in Vector3D toDirection)
        {
            var cross = Vector3D.Cross(in fromDirection, in toDirection);
            var dot = Vector3D.Dot(in fromDirection, in toDirection);
            var magnitude = (fromDirection.SqrMagnitude() * toDirection.SqrMagnitude()).Sqr();
            var quaternion = new Quaternions(cross.X, cross.Y, cross.Z, dot + magnitude);
            return quaternion.Normalized();
        }
        public void SetFromToRotation(in Vector3D fromDirection, in Vector3D toDirection) => this = FromToRotation(in fromDirection, in toDirection);
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_STANDALONE
        public static implicit operator Quaternions(UnityEngine.Quaternion value) => new(value.x, value.y, value.z, value.w);
        public static explicit operator UnityEngine.Quaternion(in Quaternions value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
#endif

        public static Quaternions operator +(in Quaternions value) => value;
        public static Quaternions operator -(in Quaternions value) => new(-value.X, -value.Y, -value.Z, -value.W);
        public static Quaternions operator +(in Quaternions lhs, in Quaternions rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
        public static Quaternions operator -(in Quaternions lhs, in Quaternions rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);

        public static Vector3D operator *(in Vector3D lhs, in Quaternions rhs) => rhs * lhs;
        public static Vector3D operator *(in Quaternions lhs, in Vector3D rhs)
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
        public static Quaternions operator *(in Quaternions lhs, in Quaternions rhs) => new()
        {
            X = lhs.X * rhs.W + lhs.W * rhs.X + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
            Y = lhs.Y * rhs.W + lhs.W * rhs.Y + lhs.Z * rhs.X - lhs.X * rhs.Z,
            Z = lhs.Z * rhs.W + lhs.W * rhs.Z + lhs.X * rhs.Y - lhs.Y * rhs.X,
            W = lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z,
        };
        public static Quaternions operator *(in Quaternions lhs, Fixed64 rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        public static Quaternions operator *(in Quaternions lhs, long rhs) => new(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
        public static Quaternions operator *(Fixed64 lhs, in Quaternions rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
        public static Quaternions operator *(long lhs, in Quaternions rhs) => new(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, lhs * rhs.W);
        public static Quaternions operator /(in Quaternions lhs, Fixed64 rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
        public static Quaternions operator /(in Quaternions lhs, long rhs) => new(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
        public static Quaternions operator /(Fixed64 lhs, in Quaternions rhs) => new(lhs / rhs.X, lhs / rhs.Y, lhs / rhs.Z, lhs / rhs.W);

        public static bool operator ==(in Quaternions lhs, in Quaternions rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z && lhs.W == rhs.W;
        public static bool operator !=(in Quaternions lhs, in Quaternions rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z || lhs.W != rhs.W;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Quaternions other && this == other;
        public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;
        public readonly bool Equals(Quaternions other) => this == other;
        public readonly int CompareTo(Quaternions other)
        {
            int match1 = X.RawValue.CompareTo(other.X.RawValue);
            if (match1 != 0)
                return match1;

            int match2 = Y.RawValue.CompareTo(other.Y.RawValue);
            if (match2 != 0)
                return match2;

            int match3 = Z.RawValue.CompareTo(other.Z.RawValue);
            if (match3 != 0)
                return match3;

            int match4 = W.RawValue.CompareTo(other.W.RawValue);
            if (match4 != 0)
                return match4;

            return 0;
        }

        public readonly override string ToString() => $"({X}, {Y}, {Z}, {W})";
        public readonly string ToString(string format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)}, {W.ToString(format)})";
        public readonly string ToString(IFormatProvider provider) => $"({X.ToString(provider)}, {Y.ToString(provider)}, {Z.ToString(provider)}, {W.ToString(provider)})";
        public readonly string ToString(string format, IFormatProvider provider) => $"({X.ToString(format, provider)}, {Y.ToString(format, provider)}, {Z.ToString(format, provider)}, {W.ToString(format, provider)})";
        #endregion
    }
}