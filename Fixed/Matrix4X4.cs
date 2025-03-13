using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的4*4矩阵
    /// </summary>
    [Serializable]
    public struct Matrix4X4 : IEquatable<Matrix4X4>, IComparable<Matrix4X4>, IFormattable
    {
        #region 字段/初始化
        public static readonly Matrix4X4 Zero = new();
        public static readonly Matrix4X4 Identity = new()
        {
            M00 = Fixed64.One,
            M11 = Fixed64.One,
            M22 = Fixed64.One,
            M33 = Fixed64.One,
        };

        public Fixed64 M00;
        public Fixed64 M01;
        public Fixed64 M02;
        public Fixed64 M03;
#if UNITY_EDITOR
        [UnityEngine.Space]
#endif
        public Fixed64 M10;
        public Fixed64 M11;
        public Fixed64 M12;
        public Fixed64 M13;
#if UNITY_EDITOR
        [UnityEngine.Space]
#endif
        public Fixed64 M20;
        public Fixed64 M21;
        public Fixed64 M22;
        public Fixed64 M23;
#if UNITY_EDITOR
        [UnityEngine.Space]
#endif
        public Fixed64 M30;
        public Fixed64 M31;
        public Fixed64 M32;
        public Fixed64 M33;

        public Matrix4X4(Fixed64 m00, Fixed64 m01, Fixed64 m02, Fixed64 m03, Fixed64 m10, Fixed64 m11, Fixed64 m12, Fixed64 m13, Fixed64 m20, Fixed64 m21, Fixed64 m22, Fixed64 m23, Fixed64 m30, Fixed64 m31, Fixed64 m32, Fixed64 m33)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M03 = m03;
            M10 = m10;
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M20 = m20;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M30 = m30;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }
        public Matrix4X4(in Vector4D m0, in Vector4D m1, in Vector4D m2, in Vector4D m3)
        {
            M00 = m0.X;
            M01 = m0.Y;
            M02 = m0.Z;
            M03 = m0.W;
            M10 = m1.X;
            M11 = m1.Y;
            M12 = m1.Z;
            M13 = m1.W;
            M20 = m2.X;
            M21 = m2.Y;
            M22 = m2.Z;
            M23 = m2.W;
            M30 = m3.X;
            M31 = m3.Y;
            M32 = m3.Z;
            M33 = m3.W;
        }

        public Fixed64 this[int row, int column]
        {
            readonly get => this[row + (column << 2)];
            set => this[row + (column << 2)] = value;
        }
        public Fixed64 this[int index]
        {
            readonly get => index switch
            {
                00 => M00,
                01 => M10,
                02 => M20,
                03 => M30,
                04 => M01,
                05 => M11,
                06 => M21,
                07 => M31,
                08 => M02,
                09 => M12,
                10 => M22,
                11 => M32,
                12 => M03,
                13 => M13,
                14 => M23,
                15 => M33,
                _ => throw new IndexOutOfRangeException($"Invalid Matrix4X4 index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 00: M00 = value; break;
                    case 01: M10 = value; break;
                    case 02: M20 = value; break;
                    case 03: M30 = value; break;
                    case 04: M01 = value; break;
                    case 05: M11 = value; break;
                    case 06: M21 = value; break;
                    case 07: M31 = value; break;
                    case 08: M02 = value; break;
                    case 09: M12 = value; break;
                    case 10: M22 = value; break;
                    case 11: M32 = value; break;
                    case 12: M03 = value; break;
                    case 13: M13 = value; break;
                    case 14: M23 = value; break;
                    case 15: M33 = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Matrix4X4 index:{index}!");
                }
            }
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 迹数，矩阵的迹
        /// </summary>
        public readonly Fixed64 Trace() => M00 + M11 + M22 + M33;
        /// <summary>
        /// 行列式
        /// </summary>
        public readonly Fixed64 Determinant()
        {
            var num0 = M22 * M33 - M23 * M32;
            var num1 = M21 * M33 - M23 * M31;
            var num2 = M21 * M32 - M22 * M31;
            var num3 = M20 * M33 - M23 * M30;
            var num4 = M20 * M32 - M22 * M30;
            var num5 = M20 * M31 - M21 * M30;
            return M00 * (M11 * num0 - M12 * num1 + M13 * num2) - M01 * (M10 * num0 - M12 * num3 + M13 * num4) + M02 * (M10 * num1 - M11 * num3 + M13 * num5) - M03 * (M10 * num2 - M11 * num4 + M12 * num5);
        }

        /// <summary>
        /// 转换向量
        /// </summary>
        public readonly Vector4D Transform(in Vector3D vector) => new()
        {
            X = vector.X * M00 + vector.Y * M01 + vector.Z * M02 + M03,
            Y = vector.X * M10 + vector.Y * M11 + vector.Z * M12 + M13,
            Z = vector.X * M20 + vector.Y * M21 + vector.Z * M22 + M23,
            W = vector.X * M30 + vector.Y * M31 + vector.Z * M32 + M33,
        };
        /// <summary>
        /// 转换向量
        /// </summary>
        public readonly Vector4D Transform(in Vector4D vector) => new()
        {
            X = vector.X * M00 + vector.Y * M01 + vector.Z * M02 + vector.W * M03,
            Y = vector.X * M10 + vector.Y * M11 + vector.Z * M12 + vector.W * M13,
            Z = vector.X * M20 + vector.Y * M21 + vector.Z * M22 + vector.W * M23,
            W = vector.X * M30 + vector.Y * M31 + vector.Z * M32 + vector.W * M33,
        };
        /// <summary>
        /// 转置转换向量
        /// </summary>
        public readonly Vector4D TransposedTransform(in Vector3D vector) => new()
        {
            X = vector.X * M00 + vector.Y * M10 + vector.Z * M20 + M30,
            Y = vector.X * M01 + vector.Y * M11 + vector.Z * M21 + M31,
            Z = vector.X * M02 + vector.Y * M12 + vector.Z * M22 + M32,
            W = vector.X * M03 + vector.Y * M13 + vector.Z * M23 + M33,
        };
        /// <summary>
        /// 转置转换向量
        /// </summary>
        public readonly Vector4D TransposedTransform(in Vector4D vector) => new()
        {
            X = vector.X * M00 + vector.Y * M10 + vector.Z * M20 + vector.W * M30,
            Y = vector.X * M01 + vector.Y * M11 + vector.Z * M21 + vector.W * M31,
            Z = vector.X * M02 + vector.Y * M12 + vector.Z * M22 + vector.W * M32,
            W = vector.X * M03 + vector.Y * M13 + vector.Z * M23 + vector.W * M33,
        };

        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Matrix4X4 Abs() => new()
        {
            M00 = M00.Abs(),
            M01 = M01.Abs(),
            M02 = M02.Abs(),
            M03 = M03.Abs(),
            M10 = M10.Abs(),
            M11 = M11.Abs(),
            M12 = M12.Abs(),
            M13 = M13.Abs(),
            M20 = M20.Abs(),
            M21 = M21.Abs(),
            M22 = M22.Abs(),
            M23 = M23.Abs(),
            M30 = M30.Abs(),
            M31 = M31.Abs(),
            M32 = M32.Abs(),
            M33 = M33.Abs(),
        };
        /// <summary>
        /// 转置
        /// </summary>
        public readonly Matrix4X4 Transpose() => new()
        {
            M00 = M00,
            M01 = M10,
            M02 = M20,
            M03 = M30,
            M10 = M01,
            M11 = M11,
            M12 = M21,
            M13 = M31,
            M20 = M02,
            M21 = M12,
            M22 = M22,
            M23 = M32,
            M30 = M03,
            M31 = M13,
            M32 = M23,
            M33 = M33,
        };
        /// <summary>
        /// 反转
        /// </summary>
        public readonly Matrix4X4 Inverse()
        {
            var determinant = Determinant();
            if (determinant == Fixed64.Zero)
                return new Matrix4X4(in Vector4D.Infinity, in Vector4D.Infinity, in Vector4D.Infinity, in Vector4D.Infinity);

            var num00 = M22 * M33 - M23 * M32;
            var num01 = M21 * M33 - M23 * M31;
            var num02 = M21 * M32 - M22 * M31;
            var num03 = M20 * M33 - M23 * M30;
            var num04 = M20 * M32 - M22 * M30;
            var num05 = M20 * M31 - M21 * M30;
            var num06 = M12 * M33 - M13 * M32;
            var num07 = M11 * M33 - M13 * M31;
            var num08 = M11 * M32 - M12 * M31;
            var num09 = M10 * M33 - M13 * M30;
            var num10 = M10 * M32 - M12 * M30;
            var num11 = M10 * M31 - M11 * M30;
            var num12 = M12 * M23 - M13 * M22;
            var num13 = M11 * M23 - M13 * M21;
            var num14 = M11 * M22 - M12 * M21;
            var num15 = M10 * M23 - M13 * M20;
            var num16 = M10 * M22 - M12 * M20;
            var num17 = M10 * M21 - M11 * M20;

            var reciprocal = determinant.Reciprocal();
            return new Matrix4X4
            {
                M00 = (M11 * num00 - M12 * num01 + M13 * num02) * reciprocal,
                M10 = (-M10 * num00 + M12 * num03 - M13 * num04) * reciprocal,
                M20 = (M10 * num01 - M11 * num03 + M13 * num05) * reciprocal,
                M30 = (-M10 * num02 + M11 * num04 - M12 * num05) * reciprocal,
                M01 = (-M01 * num00 + M02 * num01 - M03 * num02) * reciprocal,
                M11 = (M00 * num00 - M02 * num03 + M03 * num04) * reciprocal,
                M21 = (-M00 * num01 + M01 * num03 - M03 * num05) * reciprocal,
                M31 = (M00 * num02 - M01 * num04 + M02 * num05) * reciprocal,
                M02 = (M01 * num06 - M02 * num07 + M03 * num08) * reciprocal,
                M12 = (-M00 * num06 + M02 * num09 - M03 * num10) * reciprocal,
                M22 = (M00 * num07 - M01 * num09 + M03 * num11) * reciprocal,
                M32 = (-M00 * num08 + M01 * num10 - M02 * num11) * reciprocal,
                M03 = (-M01 * num12 + M02 * num13 - M03 * num14) * reciprocal,
                M13 = (M00 * num12 - M02 * num15 + M03 * num16) * reciprocal,
                M23 = (-M00 * num13 + M01 * num15 - M03 * num17) * reciprocal,
                M33 = (M00 * num14 - M01 * num16 + M02 * num17) * reciprocal,
            };
        }

        /// <summary>
        /// 位移矩阵
        /// </summary>
        public static Matrix4X4 Translate(in Vector3D translation) => Translate(translation.X, translation.Y, translation.Z);
        /// <summary>
        /// 位移矩阵
        /// </summary>
        public static Matrix4X4 Translate(Fixed64 x, Fixed64 y, Fixed64 z) => new()
        {
            M00 = Fixed64.One,
            M01 = Fixed64.Zero,
            M02 = Fixed64.Zero,
            M03 = x,
            M10 = Fixed64.Zero,
            M11 = Fixed64.One,
            M12 = Fixed64.Zero,
            M13 = y,
            M20 = Fixed64.Zero,
            M21 = Fixed64.Zero,
            M22 = Fixed64.One,
            M23 = z,
            M30 = Fixed64.Zero,
            M31 = Fixed64.Zero,
            M32 = Fixed64.Zero,
            M33 = Fixed64.One,
        };

        /// <summary>
        /// 旋转矩阵
        /// </summary>
        public static Matrix4X4 Rotate(in Quaternion rotation) => Rotate(rotation.X, rotation.Y, rotation.Z, rotation.W);
        /// <summary>
        /// 旋转矩阵
        /// </summary>
        public static Matrix4X4 Rotate(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            var x2 = x << 1;
            var y2 = y << 1;
            var z2 = z << 1;
            var xx = x * x2;
            var yy = y * y2;
            var zz = z * z2;
            var xy = x * y2;
            var xz = x * z2;
            var yz = y * z2;
            var wx = w * x2;
            var wy = w * y2;
            var wz = w * z2;
            return new Matrix4X4
            {
                M00 = Fixed64.One - yy - zz,
                M10 = xy + wz,
                M20 = xz - wy,
                M30 = Fixed64.Zero,
                M01 = xy - wz,
                M11 = Fixed64.One - xx - zz,
                M21 = yz + wx,
                M31 = Fixed64.Zero,
                M02 = xz + wy,
                M12 = yz - wx,
                M22 = Fixed64.One - xx - yy,
                M32 = Fixed64.Zero,
                M03 = Fixed64.Zero,
                M13 = Fixed64.Zero,
                M23 = Fixed64.Zero,
                M33 = Fixed64.One,
            };
        }

        /// <summary>
        /// 缩放矩阵
        /// </summary>
        public static Matrix4X4 Scale(Fixed64 scale) => Scale(scale, scale, scale);
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        public static Matrix4X4 Scale(in Vector3D scale) => Scale(scale.X, scale.Y, scale.Z);
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        public static Matrix4X4 Scale(Fixed64 scale, in Vector3D center) => Scale(scale, scale, scale, in center);
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        public static Matrix4X4 Scale(in Vector3D scale, in Vector3D center) => Scale(scale.X, scale.Y, scale.Z, in center);
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        public static Matrix4X4 Scale(Fixed64 x, Fixed64 y, Fixed64 z) => new()
        {
            M00 = x,
            M01 = Fixed64.Zero,
            M02 = Fixed64.Zero,
            M03 = Fixed64.Zero,
            M10 = Fixed64.Zero,
            M11 = y,
            M12 = Fixed64.Zero,
            M13 = Fixed64.Zero,
            M20 = Fixed64.Zero,
            M21 = Fixed64.Zero,
            M22 = z,
            M23 = Fixed64.Zero,
            M30 = Fixed64.Zero,
            M31 = Fixed64.Zero,
            M32 = Fixed64.Zero,
            M33 = Fixed64.One,
        };
        /// <summary>
        /// 缩放矩阵
        /// </summary>
        public static Matrix4X4 Scale(Fixed64 x, Fixed64 y, Fixed64 z, in Vector3D center) => new()
        {
            M00 = x,
            M01 = Fixed64.Zero,
            M02 = Fixed64.Zero,
            M03 = Fixed64.Zero,
            M10 = Fixed64.Zero,
            M11 = y,
            M12 = Fixed64.Zero,
            M13 = Fixed64.Zero,
            M20 = Fixed64.Zero,
            M21 = Fixed64.Zero,
            M22 = z,
            M23 = Fixed64.Zero,
            M30 = center.X * (Fixed64.One - x),
            M31 = center.Y * (Fixed64.One - y),
            M32 = center.Z * (Fixed64.One - z),
            M33 = Fixed64.One,
        };

        /// <summary>
        /// 平移、旋转和缩放矩阵
        /// </summary>
        public static Matrix4X4 TRS(in Vector3D translation, in Quaternion rotation, in Vector3D scale) => Translate(translation.X, translation.Y, translation.Z) * Rotate(rotation.X, rotation.Y, rotation.Z, rotation.W) * Scale(scale.X, scale.Y, scale.Z);
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_STANDALONE
        public static implicit operator Matrix4X4(in UnityEngine.Matrix4x4 value) => new(value.m00, value.m01, value.m02, value.m03, value.m10, value.m11, value.m12, value.m13, value.m20, value.m21, value.m22, value.m23, value.m30, value.m31, value.m32, value.m33);
        public static explicit operator UnityEngine.Matrix4x4(in Matrix4X4 value) => new()
        {
            m00 = (float)value.M00,
            m01 = (float)value.M01,
            m02 = (float)value.M02,
            m03 = (float)value.M03,
            m10 = (float)value.M10,
            m11 = (float)value.M11,
            m12 = (float)value.M12,
            m13 = (float)value.M13,
            m20 = (float)value.M20,
            m21 = (float)value.M21,
            m22 = (float)value.M22,
            m23 = (float)value.M23,
            m30 = (float)value.M30,
            m31 = (float)value.M31,
            m32 = (float)value.M32,
            m33 = (float)value.M33,
        };
#endif

        public static implicit operator Matrix4X4(in System.Numerics.Matrix4x4 value) => new(value.M11, value.M12, value.M13, value.M14, value.M21, value.M22, value.M23, value.M24, value.M31, value.M32, value.M33, value.M34, value.M41, value.M42, value.M43, value.M44);
        public static explicit operator System.Numerics.Matrix4x4(in Matrix4X4 value) => new((float)value.M00, (float)value.M01, (float)value.M02, (float)value.M03, (float)value.M10, (float)value.M11, (float)value.M12, (float)value.M13, (float)value.M20, (float)value.M21, (float)value.M22, (float)value.M23, (float)value.M30, (float)value.M31, (float)value.M32, (float)value.M33);

        public static Matrix4X4 operator +(in Matrix4X4 value) => value;
        public static Matrix4X4 operator -(in Matrix4X4 value) => new()
        {
            M00 = -value.M00,
            M01 = -value.M01,
            M02 = -value.M02,
            M03 = -value.M03,
            M10 = -value.M10,
            M11 = -value.M11,
            M12 = -value.M12,
            M13 = -value.M13,
            M20 = -value.M20,
            M21 = -value.M21,
            M22 = -value.M22,
            M23 = -value.M23,
            M30 = -value.M30,
            M31 = -value.M31,
            M32 = -value.M32,
            M33 = -value.M33,
        };
        public static Matrix4X4 operator +(in Matrix4X4 lhs, in Matrix4X4 rhs) => new()
        {
            M00 = lhs.M00 + rhs.M00,
            M01 = lhs.M01 + rhs.M01,
            M02 = lhs.M02 + rhs.M02,
            M03 = lhs.M03 + rhs.M03,
            M10 = lhs.M10 + rhs.M10,
            M11 = lhs.M11 + rhs.M11,
            M12 = lhs.M12 + rhs.M12,
            M13 = lhs.M13 + rhs.M13,
            M20 = lhs.M20 + rhs.M20,
            M21 = lhs.M21 + rhs.M21,
            M22 = lhs.M22 + rhs.M22,
            M23 = lhs.M23 + rhs.M23,
            M30 = lhs.M30 + rhs.M30,
            M31 = lhs.M31 + rhs.M31,
            M32 = lhs.M32 + rhs.M32,
            M33 = lhs.M33 + rhs.M33,
        };
        public static Matrix4X4 operator -(in Matrix4X4 lhs, in Matrix4X4 rhs) => new()
        {
            M00 = lhs.M00 - rhs.M00,
            M01 = lhs.M01 - rhs.M01,
            M02 = lhs.M02 - rhs.M02,
            M03 = lhs.M03 - rhs.M03,
            M10 = lhs.M10 - rhs.M10,
            M11 = lhs.M11 - rhs.M11,
            M12 = lhs.M12 - rhs.M12,
            M13 = lhs.M13 - rhs.M13,
            M20 = lhs.M20 - rhs.M20,
            M21 = lhs.M21 - rhs.M21,
            M22 = lhs.M22 - rhs.M22,
            M23 = lhs.M23 - rhs.M23,
            M30 = lhs.M30 - rhs.M30,
            M31 = lhs.M31 - rhs.M31,
            M32 = lhs.M32 - rhs.M32,
            M33 = lhs.M33 - rhs.M33,
        };

        public static Matrix4X4 operator *(in Matrix4X4 lhs, in Matrix4X4 rhs) => new()
        {
            M00 = lhs.M00 * rhs.M00 + lhs.M01 * rhs.M10 + lhs.M02 * rhs.M20 + lhs.M03 * rhs.M30,
            M01 = lhs.M00 * rhs.M01 + lhs.M01 * rhs.M11 + lhs.M02 * rhs.M21 + lhs.M03 * rhs.M31,
            M02 = lhs.M00 * rhs.M02 + lhs.M01 * rhs.M12 + lhs.M02 * rhs.M22 + lhs.M03 * rhs.M32,
            M03 = lhs.M00 * rhs.M03 + lhs.M01 * rhs.M13 + lhs.M02 * rhs.M23 + lhs.M03 * rhs.M33,
            M10 = lhs.M10 * rhs.M00 + lhs.M11 * rhs.M10 + lhs.M12 * rhs.M20 + lhs.M13 * rhs.M30,
            M11 = lhs.M10 * rhs.M01 + lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21 + lhs.M13 * rhs.M31,
            M12 = lhs.M10 * rhs.M02 + lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22 + lhs.M13 * rhs.M32,
            M13 = lhs.M10 * rhs.M03 + lhs.M11 * rhs.M13 + lhs.M12 * rhs.M23 + lhs.M13 * rhs.M33,
            M20 = lhs.M20 * rhs.M00 + lhs.M21 * rhs.M10 + lhs.M22 * rhs.M20 + lhs.M23 * rhs.M30,
            M21 = lhs.M20 * rhs.M01 + lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21 + lhs.M23 * rhs.M31,
            M22 = lhs.M20 * rhs.M02 + lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22 + lhs.M23 * rhs.M32,
            M23 = lhs.M20 * rhs.M03 + lhs.M21 * rhs.M13 + lhs.M22 * rhs.M23 + lhs.M23 * rhs.M33,
            M30 = lhs.M30 * rhs.M00 + lhs.M31 * rhs.M10 + lhs.M32 * rhs.M20 + lhs.M33 * rhs.M30,
            M31 = lhs.M30 * rhs.M01 + lhs.M31 * rhs.M11 + lhs.M32 * rhs.M21 + lhs.M33 * rhs.M31,
            M32 = lhs.M30 * rhs.M02 + lhs.M31 * rhs.M12 + lhs.M32 * rhs.M22 + lhs.M33 * rhs.M32,
            M33 = lhs.M30 * rhs.M03 + lhs.M31 * rhs.M13 + lhs.M32 * rhs.M23 + lhs.M33 * rhs.M33,
        };
        public static Matrix4X4 operator *(in Matrix4X4 lhs, Fixed64 rhs) => new()
        {
            M00 = lhs.M00 * rhs,
            M01 = lhs.M01 * rhs,
            M02 = lhs.M02 * rhs,
            M03 = lhs.M03 * rhs,
            M10 = lhs.M10 * rhs,
            M11 = lhs.M11 * rhs,
            M12 = lhs.M12 * rhs,
            M13 = lhs.M13 * rhs,
            M20 = lhs.M20 * rhs,
            M21 = lhs.M21 * rhs,
            M22 = lhs.M22 * rhs,
            M23 = lhs.M23 * rhs,
            M30 = lhs.M30 * rhs,
            M31 = lhs.M31 * rhs,
            M32 = lhs.M32 * rhs,
            M33 = lhs.M33 * rhs,
        };
        public static Matrix4X4 operator *(in Matrix4X4 lhs, long rhs) => new()
        {
            M00 = lhs.M00 * rhs,
            M01 = lhs.M01 * rhs,
            M02 = lhs.M02 * rhs,
            M03 = lhs.M03 * rhs,
            M10 = lhs.M10 * rhs,
            M11 = lhs.M11 * rhs,
            M12 = lhs.M12 * rhs,
            M13 = lhs.M13 * rhs,
            M20 = lhs.M20 * rhs,
            M21 = lhs.M21 * rhs,
            M22 = lhs.M22 * rhs,
            M23 = lhs.M23 * rhs,
            M30 = lhs.M30 * rhs,
            M31 = lhs.M31 * rhs,
            M32 = lhs.M32 * rhs,
            M33 = lhs.M33 * rhs,
        };
        public static Matrix4X4 operator *(Fixed64 lhs, in Matrix4X4 rhs) => new()
        {
            M00 = lhs * rhs.M00,
            M01 = lhs * rhs.M01,
            M02 = lhs * rhs.M02,
            M03 = lhs * rhs.M03,
            M10 = lhs * rhs.M10,
            M11 = lhs * rhs.M11,
            M12 = lhs * rhs.M12,
            M13 = lhs * rhs.M13,
            M20 = lhs * rhs.M20,
            M21 = lhs * rhs.M21,
            M22 = lhs * rhs.M22,
            M23 = lhs * rhs.M23,
            M30 = lhs * rhs.M30,
            M31 = lhs * rhs.M31,
            M32 = lhs * rhs.M32,
            M33 = lhs * rhs.M33,
        };
        public static Matrix4X4 operator *(long lhs, in Matrix4X4 rhs) => new()
        {
            M00 = lhs * rhs.M00,
            M01 = lhs * rhs.M01,
            M02 = lhs * rhs.M02,
            M03 = lhs * rhs.M03,
            M10 = lhs * rhs.M10,
            M11 = lhs * rhs.M11,
            M12 = lhs * rhs.M12,
            M13 = lhs * rhs.M13,
            M20 = lhs * rhs.M20,
            M21 = lhs * rhs.M21,
            M22 = lhs * rhs.M22,
            M23 = lhs * rhs.M23,
            M30 = lhs * rhs.M30,
            M31 = lhs * rhs.M31,
            M32 = lhs * rhs.M32,
            M33 = lhs * rhs.M33,
        };
        public static Matrix4X4 operator /(in Matrix4X4 lhs, Fixed64 rhs) => new()
        {
            M00 = lhs.M00 / rhs,
            M01 = lhs.M01 / rhs,
            M02 = lhs.M02 / rhs,
            M03 = lhs.M03 / rhs,
            M10 = lhs.M10 / rhs,
            M11 = lhs.M11 / rhs,
            M12 = lhs.M12 / rhs,
            M13 = lhs.M13 / rhs,
            M20 = lhs.M20 / rhs,
            M21 = lhs.M21 / rhs,
            M22 = lhs.M22 / rhs,
            M23 = lhs.M23 / rhs,
            M30 = lhs.M30 / rhs,
            M31 = lhs.M31 / rhs,
            M32 = lhs.M32 / rhs,
            M33 = lhs.M33 / rhs,
        };
        public static Matrix4X4 operator /(in Matrix4X4 lhs, long rhs) => new()
        {
            M00 = lhs.M00 / rhs,
            M01 = lhs.M01 / rhs,
            M02 = lhs.M02 / rhs,
            M03 = lhs.M03 / rhs,
            M10 = lhs.M10 / rhs,
            M11 = lhs.M11 / rhs,
            M12 = lhs.M12 / rhs,
            M13 = lhs.M13 / rhs,
            M20 = lhs.M20 / rhs,
            M21 = lhs.M21 / rhs,
            M22 = lhs.M22 / rhs,
            M23 = lhs.M23 / rhs,
            M30 = lhs.M30 / rhs,
            M31 = lhs.M31 / rhs,
            M32 = lhs.M32 / rhs,
            M33 = lhs.M33 / rhs,
        };

        public static bool operator ==(in Matrix4X4 lhs, in Matrix4X4 rhs) => lhs.M00 == rhs.M00 && lhs.M01 == rhs.M01 && lhs.M02 == rhs.M02 && lhs.M03 == rhs.M03 && lhs.M10 == rhs.M10 && lhs.M11 == rhs.M11 && lhs.M12 == rhs.M12 && lhs.M13 == rhs.M13 && lhs.M20 == rhs.M20 && lhs.M21 == rhs.M21 && lhs.M22 == rhs.M22 && lhs.M23 == rhs.M23 && lhs.M30 == rhs.M30 && lhs.M31 == rhs.M31 && lhs.M32 == rhs.M32 && lhs.M33 == rhs.M33;
        public static bool operator !=(in Matrix4X4 lhs, in Matrix4X4 rhs) => lhs.M00 != rhs.M00 || lhs.M01 != rhs.M01 || lhs.M02 != rhs.M02 || lhs.M03 != rhs.M03 || lhs.M10 != rhs.M10 || lhs.M11 != rhs.M11 || lhs.M12 != rhs.M12 || lhs.M13 != rhs.M13 || lhs.M20 != rhs.M20 || lhs.M21 != rhs.M21 || lhs.M22 != rhs.M22 || lhs.M23 != rhs.M23 || lhs.M30 != rhs.M30 || lhs.M31 != rhs.M31 || lhs.M32 != rhs.M32 || lhs.M33 != rhs.M33;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Matrix4X4 other && this == other;
        public readonly override int GetHashCode() => M00.GetHashCode() ^ M01.GetHashCode() ^ M02.GetHashCode() ^ M03.GetHashCode() ^ M10.GetHashCode() ^ M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^ M20.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^ M30.GetHashCode() ^ M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode();
        public readonly bool Equals(Matrix4X4 other) => this == other;
        public readonly int CompareTo(Matrix4X4 other)
        {
            int match0 = M00.RawValue.CompareTo(other.M00.RawValue);
            if (match0 != 0)
                return match0;

            int match1 = M01.RawValue.CompareTo(other.M01.RawValue);
            if (match1 != 0)
                return match1;

            int match2 = M02.RawValue.CompareTo(other.M02.RawValue);
            if (match2 != 0)
                return match2;

            int match3 = M03.RawValue.CompareTo(other.M03.RawValue);
            if (match3 != 0)
                return match3;

            int match4 = M10.RawValue.CompareTo(other.M10.RawValue);
            if (match4 != 0)
                return match4;

            int match5 = M11.RawValue.CompareTo(other.M11.RawValue);
            if (match5 != 0)
                return match5;

            int match6 = M12.RawValue.CompareTo(other.M12.RawValue);
            if (match6 != 0)
                return match6;

            int match7 = M13.RawValue.CompareTo(other.M13.RawValue);
            if (match7 != 0)
                return match7;

            int match8 = M20.RawValue.CompareTo(other.M20.RawValue);
            if (match8 != 0)
                return match8;

            int match9 = M21.RawValue.CompareTo(other.M21.RawValue);
            if (match9 != 0)
                return match9;

            int match10 = M22.RawValue.CompareTo(other.M22.RawValue);
            if (match10 != 0)
                return match10;

            int match11 = M23.RawValue.CompareTo(other.M23.RawValue);
            if (match11 != 0)
                return match11;

            int match12 = M30.RawValue.CompareTo(other.M30.RawValue);
            if (match12 != 0)
                return match12;

            int match13 = M31.RawValue.CompareTo(other.M31.RawValue);
            if (match13 != 0)
                return match13;

            int match14 = M32.RawValue.CompareTo(other.M32.RawValue);
            if (match14 != 0)
                return match14;

            int match15 = M33.RawValue.CompareTo(other.M33.RawValue);
            if (match15 != 0)
                return match15;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"({M00.ToString(format, provider)}|{M01.ToString(format, provider)}|{M02.ToString(format, provider)}|{M03.ToString(format, provider)}, {M10.ToString(format, provider)}|{M11.ToString(format, provider)}|{M12.ToString(format, provider)}|{M13.ToString(format, provider)}, {M20.ToString(format, provider)}|{M21.ToString(format, provider)}|{M22.ToString(format, provider)}|{M23.ToString(format, provider)}, {M30.ToString(format, provider)}|{M31.ToString(format, provider)}|{M32.ToString(format, provider)}|{M33.ToString(format, provider)})";
        #endregion
    }
}