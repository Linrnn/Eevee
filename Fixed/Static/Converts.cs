﻿using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 相互转换
    /// </summary>
    public readonly struct Converts
    {
        #region 返回Vector3D
        /// <summary>
        /// 返回欧拉角的角度角<br/>
        /// 左手坐标系（与Unity坐标系一致）
        /// </summary>
        public static Vector3D EulerAngles(in Quaternion quaternion)
        {
            var xSqr = quaternion.X.Sqr();
            var pitchDeg = quaternion.X * quaternion.W - quaternion.Y * quaternion.Z << 1;
            var yawNum = quaternion.Y * quaternion.W + quaternion.X * quaternion.Z;
            var yawDen = Fixed64.Half - quaternion.Y.Sqr() - xSqr;
            var rollNum = quaternion.X * quaternion.Y + quaternion.Z * quaternion.W;
            var rollDen = Fixed64.Half - xSqr - quaternion.Z.Sqr();

            var euler = new Vector3D
            {
                X = Maths.AsinDeg(pitchDeg.ClampNg1()), // Pitch：θ，俯仰角
                Y = Maths.Atan2Deg(yawNum, yawDen), // Yaw：ψ，偏航角
                Z = Maths.Atan2Deg(rollNum, rollDen), // Roll：φ，翻滚角
            };
            return MakePositive(in euler);
        }
        /// <summary>
        /// 返回欧拉角的角度角
        /// </summary>
        public static Vector3D EulerAngles(in Matrix3X3 matrix) => new()
        {
            X = -Maths.Atan2Deg(matrix.M21, matrix.M22),
            Y = -Maths.Atan2Deg(-matrix.M20, (matrix.M21.Sqr() + matrix.M22.Sqr()).Sqrt()),
            Z = -Maths.Atan2Deg(matrix.M10, matrix.M00),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3D MakePositive(in Vector3D euler)
        {
            var less = -9 / Maths.Pi / 500;
            var greater = Maths.Deg360 + less;
            return new Vector3D()
            {
                X = MakePositive(euler.X, less, greater),
                Y = MakePositive(euler.Y, less, greater),
                Z = MakePositive(euler.Z, less, greater),
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Fixed64 MakePositive(Fixed64 check, Fixed64 less, Fixed64 greater)
        {
            if (check < less)
                return check + Maths.Deg360;
            if (check > greater)
                return check - Maths.Deg360;
            return check;
        }
        #endregion

        #region 返回Quaternion
        /// <summary>
        /// 返回一个四元数，它围绕z轴旋转z度、围绕x轴旋转x度、围绕y轴旋转y度<br/>
        /// 左手坐标系（与Unity坐标系一致）
        /// </summary>
        public static Quaternion Euler(in Vector3D euler) => CreateQDeg(euler.Y, euler.X, euler.Z);
        /// <summary>
        /// 返回一个四元数，它围绕x轴旋转x度、围绕y轴旋转y度、围绕z轴旋转z度<br/>
        /// 左手坐标系（与Unity坐标系一致）
        /// </summary>
        public static Quaternion Euler(Fixed64 x, Fixed64 y, Fixed64 z) => CreateQDeg(y, x, z);

        /// <summary>
        /// 输入弧度，创建一个围绕“axis”旋转“rad”度的四元数
        /// </summary>
        public static Quaternion AngleAxisQRad(Fixed64 rad, in Vector3D axis)
        {
            var half = rad >> 1;
            var xyz = axis.Normalized() * Maths.Sin(half);
            return new Quaternion(in xyz, Maths.Cos(half));
        }
        /// <summary>
        /// 输入角度，创建一个围绕“axis”旋转“deg”度的四元数
        /// </summary>
        public static Quaternion AngleAxisQ(Fixed64 deg, in Vector3D axis)
        {
            var half = deg >> 1;
            var xyz = axis.Normalized() * Maths.SinDeg(half);
            return new Quaternion(in xyz, Maths.CosDeg(half));
        }

        /// <param name="yaw">偏航角</param>
        /// <param name="pitch">俯仰角</param>
        /// <param name="roll">翻滚角</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quaternion CreateQ(Fixed64 yaw, Fixed64 pitch, Fixed64 roll)
        {
            var rh = roll >> 1;
            var ph = pitch >> 1;
            var yh = yaw >> 1;

            var sy = Maths.Sin(yh);
            var cy = Maths.Cos(yh);
            var sp = Maths.Sin(ph);
            var cp = Maths.Cos(ph);
            var sr = Maths.Sin(rh);
            var cr = Maths.Cos(rh);

            return new Quaternion
            {
                X = cy * sp * cr + sy * cp * sr,
                Y = sy * cp * cr - cy * sp * sr,
                Z = cy * cp * sr - sy * sp * cr,
                W = cy * cp * cr + sy * sp * sr,
            };
        }
        /// <param name="yaw">偏航角</param>
        /// <param name="pitch">俯仰角</param>
        /// <param name="roll">翻滚角</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quaternion CreateQDeg(Fixed64 yaw, Fixed64 pitch, Fixed64 roll)
        {
            var yh = yaw >> 1;
            var ph = pitch >> 1;
            var rh = roll >> 1;

            var sy = Maths.SinDeg(yh);
            var cy = Maths.CosDeg(yh);
            var sp = Maths.SinDeg(ph);
            var cp = Maths.CosDeg(ph);
            var sr = Maths.SinDeg(rh);
            var cr = Maths.CosDeg(rh);

            return new Quaternion
            {
                X = cy * sp * cr + sy * cp * sr,
                Y = sy * cp * cr - cy * sp * sr,
                Z = cy * cp * sr - sy * sp * cr,
                W = cy * cp * cr + sy * sp * sr,
            };
        }
        #endregion

        #region 返回Matrix3X3
        /// <summary>
        /// 输入弧度，创建一个围绕“axis”旋转“rad”的3*3矩阵
        /// </summary>
        public static Matrix3X3 AngleM3(Fixed64 rad, in Vector3D axis)
        {
            var sin = Maths.Sin(rad);
            var cos = Maths.Cos(rad);
            var xx = axis.X.Sqr();
            var yy = axis.Y.Sqr();
            var zz = axis.Z.Sqr();
            var xy = axis.X * axis.Y;
            var xz = axis.X * axis.Z;
            var yz = axis.Y * axis.Z;
            return new Matrix3X3
            {
                M00 = xx + cos * (Fixed64.One - xx),
                M01 = xy - cos * xy + sin * axis.Z,
                M02 = xz - cos * xz - sin * axis.Y,
                M10 = xy - cos * xy - sin * axis.Z,
                M11 = yy + cos * (Fixed64.One - yy),
                M12 = yz - cos * yz + sin * axis.X,
                M20 = xz - cos * xz + sin * axis.Y,
                M21 = yz - cos * yz - sin * axis.X,
                M22 = zz + cos * (Fixed64.One - zz),
            };
        }
        /// <summary>
        /// 输入角度，创建一个围绕“axis”旋转“deg”的3*3矩阵
        /// </summary>
        public static Matrix3X3 AngleM3Deg(Fixed64 deg, in Vector3D axis)
        {
            var sin = Maths.SinDeg(deg);
            var cos = Maths.CosDeg(deg);
            var xx = axis.X.Sqr();
            var yy = axis.Y.Sqr();
            var zz = axis.Z.Sqr();
            var xy = axis.X * axis.Y;
            var xz = axis.X * axis.Z;
            var yz = axis.Y * axis.Z;
            return new Matrix3X3
            {
                M00 = xx + cos * (Fixed64.One - xx),
                M01 = xy - cos * xy + sin * axis.Z,
                M02 = xz - cos * xz - sin * axis.Y,
                M10 = xy - cos * xy - sin * axis.Z,
                M11 = yy + cos * (Fixed64.One - yy),
                M12 = yz - cos * yz + sin * axis.X,
                M20 = xz - cos * xz + sin * axis.Y,
                M21 = yz - cos * yz - sin * axis.X,
                M22 = zz + cos * (Fixed64.One - zz),
            };
        }
        /// <summary>
        /// 输入弧度，从旋转矩阵创建3*3矩阵
        /// </summary>
        public static Matrix3X3 CreateMatrix3X3(Fixed64 yaw, Fixed64 pitch, Fixed64 roll) => Matrix3X3.Rotate(CreateQ(yaw, pitch, roll));

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationXMat3(Fixed64 rad) => RotateX(Maths.Cos(rad), Maths.Sin(rad));
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationYMat3(Fixed64 rad) => RotateY(Maths.Cos(rad), Maths.Sin(rad));
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationZMat3(Fixed64 rad) => RotateZ(Maths.Cos(rad), Maths.Sin(rad));

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationXMat3Deg(Fixed64 deg) => RotateX(Maths.CosDeg(deg), Maths.SinDeg(deg));
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationYMat3Deg(Fixed64 deg) => RotateY(Maths.CosDeg(deg), Maths.SinDeg(deg));
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix3X3 RotationZMat3Deg(Fixed64 deg) => RotateZ(Maths.CosDeg(deg), Maths.SinDeg(deg));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix3X3 RotateX(Fixed64 cos, Fixed64 sin) => new()
        {
            M00 = Fixed64.One,
            M01 = default,
            M02 = default,
            M10 = default,
            M11 = cos,
            M12 = sin,
            M20 = default,
            M21 = -sin,
            M22 = cos,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix3X3 RotateY(Fixed64 cos, Fixed64 sin) => new()
        {
            M00 = cos,
            M01 = default,
            M02 = -sin,
            M10 = default,
            M11 = Fixed64.One,
            M12 = default,
            M20 = sin,
            M21 = default,
            M22 = cos,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix3X3 RotateZ(Fixed64 cos, Fixed64 sin) => new()
        {
            M00 = cos,
            M01 = sin,
            M02 = default,
            M10 = -sin,
            M11 = cos,
            M12 = default,
            M20 = default,
            M21 = default,
            M22 = Fixed64.One,
        };
        #endregion

        #region 返回Matrix4X4
        /// <summary>
        /// 输入弧度，创建一个围绕“axis”旋转“rad”的4*4矩阵
        /// </summary>
        public static Matrix4X4 AxisAngleM4(Fixed64 rad, in Vector3D axis) => AxisAngle(Maths.Sin(rad), Maths.Cos(rad), in axis);
        /// <summary>
        /// 输入角度，创建一个围绕“axis”旋转“deg”的4*4矩阵
        /// </summary>
        public static Matrix4X4 AxisAngleM4Deg(Fixed64 deg, in Vector3D axis) => AxisAngle(Maths.SinDeg(deg), Maths.CosDeg(deg), in axis);

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMat4(Fixed64 rad) => RotateX(Maths.Sin(rad), Maths.Cos(rad), default, default);
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMat4(Fixed64 rad) => RotateY(Maths.Sin(rad), Maths.Cos(rad), default, default);
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateMat4(Fixed64 rad) => RotateZ(Maths.Sin(rad), Maths.Cos(rad), default, default);

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMat4Deg(Fixed64 deg) => RotateX(Maths.SinDeg(deg), Maths.CosDeg(deg), default, default);
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMat4Deg(Fixed64 deg) => RotateY(Maths.SinDeg(deg), Maths.CosDeg(deg), default, default);
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMat4Deg(Fixed64 deg) => RotateZ(Maths.SinDeg(deg), Maths.CosDeg(deg), default, default);

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMat4(Fixed64 rad, in Vector3D position)
        {
            var sin = Maths.Sin(rad);
            var cos = Maths.Cos(rad);
            var one = Fixed64.One - cos;
            var m31 = position.Y * one + position.Z * sin;
            var m32 = position.Z * one - position.Y * sin;
            return RotateX(sin, cos, m31, m32);
        }
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMat4(Fixed64 rad, in Vector3D position)
        {
            var sin = Maths.Sin(rad);
            var cos = Maths.Cos(rad);
            var one = Fixed64.One - cos;
            var m30 = position.X * one - position.Z * sin;
            var m32 = position.X * one + position.X * sin;
            return RotateY(sin, cos, m30, m32);
        }
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMat4(Fixed64 rad, in Vector3D position)
        {
            var sin = Maths.Sin(rad);
            var cos = Maths.Cos(rad);
            var one = Fixed64.One - cos;
            var m30 = position.X * one + position.Y * sin;
            var m31 = position.Y * one - position.X * sin;
            return RotateZ(sin, cos, m30, m31);
        }

        /// <summary>
        /// 创建一个绕x轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateXMat4Deg(Fixed64 deg, in Vector3D position)
        {
            var sin = Maths.SinDeg(deg);
            var cos = Maths.CosDeg(deg);
            var one = Fixed64.One - cos;
            var m31 = position.Y * one + position.Z * sin;
            var m32 = position.Z * one - position.Y * sin;
            return RotateX(sin, cos, m31, m32);
        }
        /// <summary>
        /// 创建一个绕y轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateYMat4Deg(Fixed64 deg, in Vector3D position)
        {
            var sin = Maths.SinDeg(deg);
            var cos = Maths.CosDeg(deg);
            var one = Fixed64.One - cos;
            var m30 = position.X * one - position.Z * sin;
            var m32 = position.X * one + position.X * sin;
            return RotateY(sin, cos, m30, m32);
        }
        /// <summary>
        /// 创建一个绕z轴旋转点的矩阵
        /// </summary>
        public static Matrix4X4 RotateZMat4Deg(Fixed64 deg, in Vector3D position)
        {
            var sin = Maths.SinDeg(deg);
            var cos = Maths.CosDeg(deg);
            var one = Fixed64.One - cos;
            var m30 = position.X * one + position.Y * sin;
            var m31 = position.Y * one - position.X * sin;
            return RotateZ(sin, cos, m30, m31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix4X4 AxisAngle(Fixed64 sin, Fixed64 cos, in Vector3D axis)
        {
            var xx = axis.X.Sqr();
            var yy = axis.Y.Sqr();
            var zz = axis.Z.Sqr();
            var xy = axis.X * axis.Y;
            var xz = axis.X * axis.Z;
            var yz = axis.Y * axis.Z;
            return new Matrix4X4
            {
                M00 = xx + cos * (Fixed64.One - xx),
                M01 = xy - cos * xy + sin * axis.Z,
                M02 = xz - cos * xz - sin * axis.Y,
                M03 = default,
                M10 = xy - cos * xy - sin * axis.Z,
                M11 = yy + cos * (Fixed64.One - yy),
                M12 = yz - cos * yz + sin * axis.X,
                M13 = default,
                M20 = xz - cos * xz + sin * axis.Y,
                M21 = yz - cos * yz - sin * axis.X,
                M22 = zz + cos * (Fixed64.One - zz),
                M23 = default,
                M30 = default,
                M31 = default,
                M32 = default,
                M33 = Fixed64.One,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix4X4 RotateX(Fixed64 sin, Fixed64 cos, Fixed64 m31, Fixed64 m32) => new()
        {
            M00 = Fixed64.One,
            M01 = default,
            M02 = default,
            M03 = default,
            M10 = default,
            M11 = cos,
            M12 = sin,
            M13 = default,
            M20 = default,
            M21 = -sin,
            M22 = cos,
            M23 = default,
            M30 = default,
            M31 = m31,
            M32 = m32,
            M33 = Fixed64.One,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix4X4 RotateY(Fixed64 sin, Fixed64 cos, Fixed64 m30, Fixed64 m32) => new()
        {
            M00 = cos,
            M01 = default,
            M02 = -sin,
            M03 = default,
            M10 = default,
            M11 = Fixed64.One,
            M12 = default,
            M13 = default,
            M20 = sin,
            M21 = default,
            M22 = cos,
            M23 = default,
            M30 = m30,
            M31 = default,
            M32 = m32,
            M33 = Fixed64.One,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix4X4 RotateZ(Fixed64 sin, Fixed64 cos, Fixed64 m30, Fixed64 m31) => new()
        {
            M00 = cos,
            M01 = sin,
            M02 = default,
            M03 = default,
            M10 = -sin,
            M11 = cos,
            M12 = default,
            M13 = default,
            M20 = default,
            M21 = default,
            M22 = Fixed64.One,
            M23 = default,
            M30 = m30,
            M31 = m31,
            M32 = default,
            M33 = Fixed64.One,
        };
        #endregion

        #region 返回几何类型
        public static Rectangle AsRectangle(in AABB2D value) => new(value.Min(), value.Size());
        public static Rectangle AsRectangle(in AABB2DInt value) => new(value.Min(), value.Size());

        public static Circle AsCircle(in AABB2DInt value) // “AABB”的宽高需要相等
        {
            Assert.Equal<InvalidOperationException, AssertArgs<Fixed64, Fixed64>, Fixed64>(value.W, value.H, nameof(value), "as fail, W:{0} != H:{1}", new AssertArgs<Fixed64, Fixed64>(value.W, value.H));
            return new Circle(value.X, value.Y, value.W);
        }
        public static CircleInt AsCircleInt(in AABB2DInt value) // “AABB”的宽高需要相等
        {
            Assert.Equal<InvalidOperationException, AssertArgs<int, int>, int>(value.W, value.H, nameof(value), "as fail, W:{0} != H:{1}", new AssertArgs<int, int>(value.W, value.H));
            return new CircleInt(value.X, value.Y, value.W);
        }

        public static AABB2D AsAABB2D(in Rectangle value) => new(value.Center, value.Width >> 1, value.Height >> 1);
        public static AABB2D AsAABB2D(in Vector2D value) => new(value.X, value.Y, Fixed64.Zero);
        public static AABB2D AsAABB2D(in Circle value) => new(value.X, value.Y, value.R); // 外接AABB
        public static AABB2D AsAABB2D(in CircleInt value) => new(value.X, value.Y, value.R); // 外接AABB
        public static AABB2D AsAABB2D(in OBB2D value) => new(value.X, value.Y, value.RotatedHalfSize()); // 外接AABB
        public static AABB2D AsAABB2D(in OBB2DInt value) => new(value.X, value.Y, value.RotatedHalfSize()); // 外接AABB
        public static AABB2D AsAABB2DWithoutAngle(in OBB2D value) => new(value.X, value.Y, value.W, value.H);
        public static AABB2DInt AsAABB2DInt(Vector2DInt value) => new(value.X, value.Y, 0);
        public static AABB2DInt AsAABB2DInt(in CircleInt value) => new(value.X, value.Y, value.R); // 外接AABB
        public static AABB2DInt AsAABB2DInt(in OBB2DInt value) // 外接AABB
        {
            var extents = value.RotatedHalfSize();
            return new AABB2DInt(value.X, value.Y, (int)extents.X, (int)extents.Y);
        }
        public static AABB2DInt AsAABB2DIntWithoutAngle(in OBB2DInt value) => new(value.X, value.Y, value.W, value.H);
        #endregion
    }
}