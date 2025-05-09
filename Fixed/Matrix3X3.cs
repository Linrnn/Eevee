﻿using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的3*3矩阵
    /// </summary>
    [Serializable]
    public struct Matrix3X3 : IEquatable<Matrix3X3>, IComparable<Matrix3X3>, IFormattable
    {
        #region 字段/初始化
        public static readonly Matrix3X3 Zero = new();
        public static readonly Matrix3X3 Identity = new()
        {
            M00 = Fixed64.One,
            M11 = Fixed64.One,
            M22 = Fixed64.One,
        };

        public Fixed64 M00;
        public Fixed64 M01;
        public Fixed64 M02;
#if UNITY_EDITOR
        [UnityEngine.Space]
#endif
        public Fixed64 M10;
        public Fixed64 M11;
        public Fixed64 M12;
#if UNITY_EDITOR
        [UnityEngine.Space]
#endif
        public Fixed64 M20;
        public Fixed64 M21;
        public Fixed64 M22;

        public Matrix3X3(Fixed64 m00, Fixed64 m01, Fixed64 m02, Fixed64 m10, Fixed64 m11, Fixed64 m12, Fixed64 m20, Fixed64 m21, Fixed64 m22)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M10 = m10;
            M11 = m11;
            M12 = m12;
            M20 = m20;
            M21 = m21;
            M22 = m22;
        }
        public Matrix3X3(in Vector3D m0, in Vector3D m1, in Vector3D m2)
        {
            M00 = m0.X;
            M01 = m0.Y;
            M02 = m0.Z;
            M10 = m1.X;
            M11 = m1.Y;
            M12 = m1.Z;
            M20 = m2.X;
            M21 = m2.Y;
            M22 = m2.Z;
        }

        public Fixed64 this[int row, int column]
        {
            readonly get => this[row + column * 3];
            set => this[row + column * 3] = value;
        }
        public Fixed64 this[int index]
        {
            readonly get => index switch
            {
                0 => M00,
                1 => M10,
                2 => M20,
                3 => M01,
                4 => M11,
                5 => M21,
                6 => M02,
                7 => M12,
                8 => M22,
                _ => throw new IndexOutOfRangeException($"Invalid Matrix3X3 index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: M00 = value; break;
                    case 1: M10 = value; break;
                    case 2: M20 = value; break;
                    case 3: M01 = value; break;
                    case 4: M11 = value; break;
                    case 5: M21 = value; break;
                    case 6: M02 = value; break;
                    case 7: M12 = value; break;
                    case 8: M22 = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Matrix3X3 index:{index}!");
                }
            }
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 迹数，矩阵的迹
        /// </summary>
        public readonly Fixed64 Trace() => M00 + M11 + M22;
        /// <summary>
        /// 模长的平方
        /// </summary>
        public readonly Fixed64 SqrMagnitude() => M00.Sqr() + M01.Sqr() + M02.Sqr() + M10.Sqr() + M11.Sqr() + M12.Sqr() + M20.Sqr() + M21.Sqr() + M22.Sqr();
        /// <summary>
        /// 模长
        /// </summary>
        public readonly Fixed64 Magnitude() => SqrMagnitude().Sqrt();
        /// <summary>
        /// 行列式
        /// </summary>
        public readonly Fixed64 Determinant() => M00 * M11 * M22 + M01 * M12 * M20 + M02 * M10 * M21 - M02 * M11 * M20 - M00 * M12 * M21 - M01 * M10 * M22;

        /// <summary>
        /// 转换向量
        /// </summary>
        public readonly Vector3D Transform(in Vector3D vector) => new()
        {
            X = vector.X * M00 + vector.Y * M10 + vector.Z * M20,
            Y = vector.X * M01 + vector.Y * M11 + vector.Z * M21,
            Z = vector.X * M02 + vector.Y * M12 + vector.Z * M22,
        };
        /// <summary>
        /// 转置转换向量
        /// </summary>
        public readonly Vector3D TransposedTransform(in Vector3D vector) => new()
        {
            X = vector.X * M00 + vector.Y * M01 + vector.Z * M02,
            Y = vector.X * M10 + vector.Y * M11 + vector.Z * M12,
            Z = vector.X * M20 + vector.Y * M21 + vector.Z * M22,
        };

        /// <summary>
        /// 转换成四元数
        /// </summary>
        public readonly Quaternion AsQuaternion()
        {
            var trace = Trace();

            if (trace > Fixed64.Zero)
            {
                var sqrt = (trace + Fixed64.One).Sqrt();
                var half = Fixed64.Half / sqrt;
                return new Quaternion
                {
                    W = sqrt >> 1,
                    X = (M12 - M21) * half,
                    Y = (M20 - M02) * half,
                    Z = (M01 - M10) * half,
                };
            }

            if (M00 >= M11 && M00 >= M22)
            {
                var sqrt = (Fixed64.One + M00 - M11 - M22).Sqrt();
                var half = Fixed64.Half / sqrt;
                return new Quaternion
                {
                    X = sqrt >> 1,
                    Y = (M01 + M10) * half,
                    Z = (M20 + M02) * half,
                    W = (M12 - M21) * half,
                };
            }

            if (M11 > M22)
            {
                var sqrt = (Fixed64.One + M11 - M00 - M22).Sqrt();
                var half = Fixed64.Half / sqrt;
                return new Quaternion
                {
                    X = (M01 + M10) * half,
                    Y = sqrt >> 1,
                    Z = (M21 + M12) * half,
                    W = (M20 - M02) * half,
                };
            }

            {
                var sqrt = (Fixed64.One + M22 - M00 - M11).Sqrt();
                var half = Fixed64.Half / sqrt;
                return new Quaternion
                {
                    X = (M20 + M02) * half,
                    Y = (M21 + M12) * half,
                    Z = sqrt >> 1,
                    W = (M01 - M10) * half,
                };
            }
        }
        /// <summary>
        /// 使用指定的“forward”和“upwards”方向创建四元数
        /// </summary>
        public static Quaternion LookRotation(in Vector3D forward) => LookRotation(in forward, in Vector3D.Up);
        /// <summary>
        /// 使用指定的“forward”和“upwards”方向创建四元数
        /// </summary>
        public static Quaternion LookRotation(in Vector3D forward, in Vector3D upwards) => LookAt(in forward, in upwards).AsQuaternion();

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Matrix3X3 Abs() => new()
        {
            M00 = M00.Abs(),
            M01 = M01.Abs(),
            M02 = M02.Abs(),
            M10 = M10.Abs(),
            M11 = M11.Abs(),
            M12 = M12.Abs(),
            M20 = M20.Abs(),
            M21 = M21.Abs(),
            M22 = M22.Abs(),
        };
        /// <summary>
        /// 转置
        /// </summary>
        public readonly Matrix3X3 Transpose() => new()
        {
            M00 = M00,
            M01 = M10,
            M02 = M20,
            M10 = M01,
            M11 = M11,
            M12 = M21,
            M20 = M02,
            M21 = M12,
            M22 = M22,
        };
        /// <summary>
        /// 反转
        /// </summary>
        public readonly bool Inverse(out Matrix3X3 matrix)
        {
            var determinant = Determinant();
            if (determinant.RawValue == 0)
            {
                matrix = new Matrix3X3(in Vector3D.Infinity, in Vector3D.Infinity, in Vector3D.Infinity);
                return false;
            }

            matrix = new Matrix3X3 // “Fixed64.Reciprocal()”在大数运算下，会丢失精度
            {
                M00 = (M11 * M22 - M12 * M21) / determinant,
                M01 = (M02 * M21 - M01 * M22) / determinant,
                M02 = (M01 * M12 - M11 * M02) / determinant,
                M10 = (M12 * M20 - M22 * M10) / determinant,
                M11 = (M00 * M22 - M20 * M02) / determinant,
                M12 = (M02 * M10 - M12 * M00) / determinant,
                M20 = (M10 * M21 - M20 * M11) / determinant,
                M21 = (M01 * M20 - M21 * M00) / determinant,
                M22 = (M00 * M11 - M10 * M01) / determinant,
            };
            return true;
        }
        /// <summary>
        /// 创建“LookAt”矩阵
        /// </summary>
        public static Matrix3X3 LookAtUp(in Vector3D from, in Vector3D to) => LookAt(to - from, in Vector3D.Up);
        /// <summary>
        /// 创建“LookAt”矩阵
        /// </summary>
        public static Matrix3X3 LookAt(in Vector3D forward, in Vector3D upwards)
        {
            var m2 = forward.Normalized();
            var m0 = Vector3D.Cross(in upwards, in m2).Normalized();
            var m1 = Vector3D.Cross(in m2, in m0);
            return new Matrix3X3(in m0, in m1, in m2);
        }
        /// <summary>
        /// 输入四元数，从旋转矩阵创建3*3矩阵
        /// </summary>
        public static Matrix3X3 Rotate(in Quaternion quaternion)
        {
            var xx = quaternion.X.Sqr();
            var yy = quaternion.Y.Sqr();
            var zz = quaternion.Z.Sqr();
            var xy = quaternion.X * quaternion.Y;
            var zw = quaternion.Z * quaternion.W;
            var xz = quaternion.X * quaternion.Z;
            var yw = quaternion.Y * quaternion.W;
            var yz = quaternion.Y * quaternion.Z;
            var xw = quaternion.X * quaternion.W;
            return new Matrix3X3
            {
                M00 = Fixed64.One - (yy + zz << 1),
                M01 = xy + zw << 1,
                M02 = xz - yw << 1,
                M10 = xy - zw << 1,
                M11 = Fixed64.One - (xx + zz << 1),
                M12 = yz + xw << 1,
                M20 = xz + yw << 1,
                M21 = yz - xw << 1,
                M22 = Fixed64.One - (xx + yy << 1),
            };
        }
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static Matrix3X3 operator +(in Matrix3X3 value) => value;
        public static Matrix3X3 operator -(in Matrix3X3 value) => new()
        {
            M00 = -value.M00,
            M01 = -value.M01,
            M02 = -value.M02,
            M10 = -value.M10,
            M11 = -value.M11,
            M12 = -value.M12,
            M20 = -value.M20,
            M21 = -value.M21,
            M22 = -value.M22,
        };
        public static Matrix3X3 operator +(in Matrix3X3 lhs, in Matrix3X3 rhs) => new()
        {
            M00 = lhs.M00 + rhs.M00,
            M01 = lhs.M01 + rhs.M01,
            M02 = lhs.M02 + rhs.M02,
            M10 = lhs.M10 + rhs.M10,
            M11 = lhs.M11 + rhs.M11,
            M12 = lhs.M12 + rhs.M12,
            M20 = lhs.M20 + rhs.M20,
            M21 = lhs.M21 + rhs.M21,
            M22 = lhs.M22 + rhs.M22,
        };
        public static Matrix3X3 operator -(in Matrix3X3 lhs, in Matrix3X3 rhs) => new()
        {
            M00 = lhs.M00 - rhs.M00,
            M01 = lhs.M01 - rhs.M01,
            M02 = lhs.M02 - rhs.M02,
            M10 = lhs.M10 - rhs.M10,
            M11 = lhs.M11 - rhs.M11,
            M12 = lhs.M12 - rhs.M12,
            M20 = lhs.M20 - rhs.M20,
            M21 = lhs.M21 - rhs.M21,
            M22 = lhs.M22 - rhs.M22,
        };

        public static Matrix3X3 operator *(in Matrix3X3 lhs, in Matrix3X3 rhs) => new()
        {
            M00 = lhs.M00 * rhs.M00 + lhs.M01 * rhs.M10 + lhs.M02 * rhs.M20,
            M01 = lhs.M00 * rhs.M01 + lhs.M01 * rhs.M11 + lhs.M02 * rhs.M21,
            M02 = lhs.M00 * rhs.M02 + lhs.M01 * rhs.M12 + lhs.M02 * rhs.M22,
            M10 = lhs.M10 * rhs.M00 + lhs.M11 * rhs.M10 + lhs.M12 * rhs.M20,
            M11 = lhs.M10 * rhs.M01 + lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21,
            M12 = lhs.M10 * rhs.M02 + lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22,
            M20 = lhs.M20 * rhs.M00 + lhs.M21 * rhs.M10 + lhs.M22 * rhs.M20,
            M21 = lhs.M20 * rhs.M01 + lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21,
            M22 = lhs.M20 * rhs.M02 + lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22,
        };
        public static Matrix3X3 operator *(in Matrix3X3 lhs, Fixed64 rhs) => new()
        {
            M00 = lhs.M00 * rhs,
            M01 = lhs.M01 * rhs,
            M02 = lhs.M02 * rhs,
            M10 = lhs.M10 * rhs,
            M11 = lhs.M11 * rhs,
            M12 = lhs.M12 * rhs,
            M20 = lhs.M20 * rhs,
            M21 = lhs.M21 * rhs,
            M22 = lhs.M22 * rhs,
        };
        public static Matrix3X3 operator *(in Matrix3X3 lhs, long rhs) => new()
        {
            M00 = lhs.M00 * rhs,
            M01 = lhs.M01 * rhs,
            M02 = lhs.M02 * rhs,
            M10 = lhs.M10 * rhs,
            M11 = lhs.M11 * rhs,
            M12 = lhs.M12 * rhs,
            M20 = lhs.M20 * rhs,
            M21 = lhs.M21 * rhs,
            M22 = lhs.M22 * rhs,
        };
        public static Matrix3X3 operator *(Fixed64 lhs, in Matrix3X3 rhs) => new()
        {
            M00 = lhs * rhs.M00,
            M01 = lhs * rhs.M01,
            M02 = lhs * rhs.M02,
            M10 = lhs * rhs.M10,
            M11 = lhs * rhs.M11,
            M12 = lhs * rhs.M12,
            M20 = lhs * rhs.M20,
            M21 = lhs * rhs.M21,
            M22 = lhs * rhs.M22,
        };
        public static Matrix3X3 operator *(long lhs, in Matrix3X3 rhs) => new()
        {
            M00 = lhs * rhs.M00,
            M01 = lhs * rhs.M01,
            M02 = lhs * rhs.M02,
            M10 = lhs * rhs.M10,
            M11 = lhs * rhs.M11,
            M12 = lhs * rhs.M12,
            M20 = lhs * rhs.M20,
            M21 = lhs * rhs.M21,
            M22 = lhs * rhs.M22,
        };
        public static Matrix3X3 operator /(in Matrix3X3 lhs, Fixed64 rhs) => new() // “Fixed64.Reciprocal()”在大数运算下，会丢失精度
        {
            M00 = lhs.M00 / rhs,
            M01 = lhs.M01 / rhs,
            M02 = lhs.M02 / rhs,
            M10 = lhs.M10 / rhs,
            M11 = lhs.M11 / rhs,
            M12 = lhs.M12 / rhs,
            M20 = lhs.M20 / rhs,
            M21 = lhs.M21 / rhs,
            M22 = lhs.M22 / rhs,
        };
        public static Matrix3X3 operator /(in Matrix3X3 lhs, long rhs) => new()
        {
            M00 = lhs.M00 / rhs,
            M01 = lhs.M01 / rhs,
            M02 = lhs.M02 / rhs,
            M10 = lhs.M10 / rhs,
            M11 = lhs.M11 / rhs,
            M12 = lhs.M12 / rhs,
            M20 = lhs.M20 / rhs,
            M21 = lhs.M21 / rhs,
            M22 = lhs.M22 / rhs,
        };

        public static bool operator ==(in Matrix3X3 lhs, in Matrix3X3 rhs) => lhs.M00 == rhs.M00 && lhs.M01 == rhs.M01 && lhs.M02 == rhs.M02 && lhs.M10 == rhs.M10 && lhs.M11 == rhs.M11 && lhs.M12 == rhs.M12 && lhs.M20 == rhs.M20 && lhs.M21 == rhs.M21 && lhs.M22 == rhs.M22;
        public static bool operator !=(in Matrix3X3 lhs, in Matrix3X3 rhs) => lhs.M00 != rhs.M00 || lhs.M01 != rhs.M01 || lhs.M02 != rhs.M02 || lhs.M10 != rhs.M10 || lhs.M11 != rhs.M11 || lhs.M12 != rhs.M12 || lhs.M20 != rhs.M20 || lhs.M21 != rhs.M21 || lhs.M22 != rhs.M22;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Matrix3X3 other && this == other;
        public readonly override int GetHashCode() => M00.GetHashCode() ^ M01.GetHashCode() ^ M02.GetHashCode() ^ M10.GetHashCode() ^ M11.GetHashCode() ^ M12.GetHashCode() ^ M20.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode();
        public readonly bool Equals(Matrix3X3 other) => this == other;
        public readonly int CompareTo(Matrix3X3 other)
        {
            int match0 = M00.CompareTo(other.M00);
            if (match0 != 0)
                return match0;

            int match1 = M01.CompareTo(other.M01);
            if (match1 != 0)
                return match1;

            int match2 = M02.CompareTo(other.M02);
            if (match2 != 0)
                return match2;

            int match3 = M10.CompareTo(other.M10);
            if (match3 != 0)
                return match3;

            int match4 = M11.CompareTo(other.M11);
            if (match4 != 0)
                return match4;

            int match5 = M12.CompareTo(other.M12);
            if (match5 != 0)
                return match5;

            int match6 = M20.CompareTo(other.M20);
            if (match6 != 0)
                return match6;

            int match7 = M21.CompareTo(other.M21);
            if (match7 != 0)
                return match7;

            int match8 = M22.CompareTo(other.M22);
            if (match8 != 0)
                return match8;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"({M00.ToString(format, provider)}, {M01.ToString(format, provider)}, {M02.ToString(format, provider)} | {M10.ToString(format, provider)}, {M11.ToString(format, provider)}, {M12.ToString(format, provider)} | {M20.ToString(format, provider)}, {M21.ToString(format, provider)}, {M22.ToString(format, provider)})";
        #endregion
    }
}