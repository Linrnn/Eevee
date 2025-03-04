﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty.  In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *
 *  1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *      misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
 */

using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 3x3 Matrix.
    /// </summary>
    [Serializable]
    public struct Matrix3X3 : IEquatable<Matrix3X3>, IComparable<Matrix3X3>
    {
        /// <summary>
        /// M11
        /// </summary>
        public Fixed64 M11; // 1st row vector
        /// <summary>
        /// M12
        /// </summary>
        public Fixed64 M12;
        /// <summary>
        /// M13
        /// </summary>
        public Fixed64 M13;
        /// <summary>
        /// M21
        /// </summary>
        public Fixed64 M21; // 2nd row vector
        /// <summary>
        /// M22
        /// </summary>
        public Fixed64 M22;
        /// <summary>
        /// M23
        /// </summary>
        public Fixed64 M23;
        /// <summary>
        /// M31
        /// </summary>
        public Fixed64 M31; // 3rd row vector
        /// <summary>
        /// M32
        /// </summary>
        public Fixed64 M32;
        /// <summary>
        /// M33
        /// </summary>
        public Fixed64 M33;

        internal static Matrix3X3 InternalIdentity;

        /// <summary>
        /// Identity matrix.
        /// </summary>
        public static readonly Matrix3X3 Identity;
        public static readonly Matrix3X3 Zero;

        static Matrix3X3()
        {
            Zero = new Matrix3X3();

            Identity = new Matrix3X3();
            Identity.M11 = Fixed64.One;
            Identity.M22 = Fixed64.One;
            Identity.M33 = Fixed64.One;

            InternalIdentity = Identity;
        }

        public Vector3D eulerAngles
        {
            get
            {
                var result = new Vector3D();
                result.X = -Maths.Atan2Deg(M32, M33);
                result.Y = -Maths.Atan2Deg(-M31, (M32.Sqr() + M33.Sqr()).Sqrt());
                result.Z = -Maths.Atan2Deg(M21, M11);
                return result;
            }
        }

        public static Matrix3X3 CreateFromYawPitchRoll(Fixed64 yaw, Fixed64 pitch, Fixed64 roll)
        {
            Quaternions.CreateFromYawPitchRoll(yaw, pitch, roll, out var quaternion);
            CreateFromQuaternion(ref quaternion, out var matrix);
            return matrix;
        }

        public static Matrix3X3 CreateRotationX(Fixed64 radians)
        {
            var cos = Maths.Cos(radians);
            var sin = Maths.Sin(radians);

            Matrix3X3 matrix;
            matrix.M11 = Fixed64.One;
            matrix.M12 = Fixed64.Zero;
            matrix.M13 = Fixed64.Zero;
            matrix.M21 = Fixed64.Zero;
            matrix.M22 = cos;
            matrix.M23 = sin;
            matrix.M31 = Fixed64.Zero;
            matrix.M32 = -sin;
            matrix.M33 = cos;
            return matrix;
        }

        public static void CreateRotationX(Fixed64 radians, out Matrix3X3 result)
        {
            var cos = Maths.Cos(radians);
            var sin = Maths.Sin(radians);

            result.M11 = Fixed64.One;
            result.M12 = Fixed64.Zero;
            result.M13 = Fixed64.Zero;
            result.M21 = Fixed64.Zero;
            result.M22 = cos;
            result.M23 = sin;
            result.M31 = Fixed64.Zero;
            result.M32 = -sin;
            result.M33 = cos;
        }

        public static Matrix3X3 CreateRotationY(Fixed64 radians)
        {
            var cos = Maths.Cos(radians);
            var sin = Maths.Sin(radians);

            Matrix3X3 matrix;
            matrix.M11 = cos;
            matrix.M12 = Fixed64.Zero;
            matrix.M13 = -sin;
            matrix.M21 = Fixed64.Zero;
            matrix.M22 = Fixed64.One;
            matrix.M23 = Fixed64.Zero;
            matrix.M31 = sin;
            matrix.M32 = Fixed64.Zero;
            matrix.M33 = cos;
            return matrix;
        }

        public static void CreateRotationY(Fixed64 radians, out Matrix3X3 result)
        {
            var cos = Maths.Cos(radians);
            var sin = Maths.Sin(radians);

            result.M11 = cos;
            result.M12 = Fixed64.Zero;
            result.M13 = -sin;
            result.M21 = Fixed64.Zero;
            result.M22 = Fixed64.One;
            result.M23 = Fixed64.Zero;
            result.M31 = sin;
            result.M32 = Fixed64.Zero;
            result.M33 = cos;
        }

        public static Matrix3X3 CreateRotationZ(Fixed64 radians)
        {
            var cos = Maths.Cos(radians);
            var sin = Maths.Sin(radians);

            Matrix3X3 matrix;
            matrix.M11 = cos;
            matrix.M12 = sin;
            matrix.M13 = Fixed64.Zero;
            matrix.M21 = -sin;
            matrix.M22 = cos;
            matrix.M23 = Fixed64.Zero;
            matrix.M31 = Fixed64.Zero;
            matrix.M32 = Fixed64.Zero;
            matrix.M33 = Fixed64.One;
            return matrix;
        }

        public static void CreateRotationZ(Fixed64 radians, out Matrix3X3 result)
        {
            var cos = Maths.Cos(radians);
            var sin = Maths.Sin(radians);

            result.M11 = cos;
            result.M12 = sin;
            result.M13 = Fixed64.Zero;
            result.M21 = -sin;
            result.M22 = cos;
            result.M23 = Fixed64.Zero;
            result.M31 = Fixed64.Zero;
            result.M32 = Fixed64.Zero;
            result.M33 = Fixed64.One;
        }

        /// <summary>
        /// Initializes a new instance of the matrix structure.
        /// </summary>
        /// <param name="m11">m11</param>
        /// <param name="m12">m12</param>
        /// <param name="m13">m13</param>
        /// <param name="m21">m21</param>
        /// <param name="m22">m22</param>
        /// <param name="m23">m23</param>
        /// <param name="m31">m31</param>
        /// <param name="m32">m32</param>
        /// <param name="m33">m33</param>

        #region public JMatrix(FP m11, FP m12, FP m13, FP m21, FP m22, FP m23,FP m31, FP m32, FP m33)
        public Matrix3X3(Fixed64 m11, Fixed64 m12, Fixed64 m13, Fixed64 m21, Fixed64 m22, Fixed64 m23, Fixed64 m31, Fixed64 m32, Fixed64 m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }
        #endregion

        /// <summary>
        /// Gets the determinant of the matrix.
        /// </summary>
        /// <returns>The determinant of the matrix.</returns>

        #region public FP Determinant()
        //public FP Determinant()
        //{
        //    return M11 * M22 * M33 -M11 * M23 * M32 -M12 * M21 * M33 +M12 * M23 * M31 + M13 * M21 * M32 - M13 * M22 * M31;
        //}
        #endregion

        /// <summary>
        /// Multiply two matrices. Notice: matrix multiplication is not commutative.
        /// </summary>
        /// <param name="matrix1">The first matrix.</param>
        /// <param name="matrix2">The second matrix.</param>
        /// <returns>The product of both matrices.</returns>

        #region public static JMatrix Multiply(JMatrix matrix1, JMatrix matrix2)
        public static Matrix3X3 Multiply(Matrix3X3 matrix1, Matrix3X3 matrix2)
        {
            Matrix3X3 result;
            Matrix3X3.Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }

        /// <summary>
        /// Multiply two matrices. Notice: matrix multiplication is not commutative.
        /// </summary>
        /// <param name="matrix1">The first matrix.</param>
        /// <param name="matrix2">The second matrix.</param>
        /// <param name="result">The product of both matrices.</param>
        public static void Multiply(ref Matrix3X3 matrix1, ref Matrix3X3 matrix2, out Matrix3X3 result)
        {
            Fixed64 num0 = ((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31);
            Fixed64 num1 = ((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32);
            Fixed64 num2 = ((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33);
            Fixed64 num3 = ((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31);
            Fixed64 num4 = ((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32);
            Fixed64 num5 = ((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33);
            Fixed64 num6 = ((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31);
            Fixed64 num7 = ((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32);
            Fixed64 num8 = ((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33);

            result.M11 = num0;
            result.M12 = num1;
            result.M13 = num2;
            result.M21 = num3;
            result.M22 = num4;
            result.M23 = num5;
            result.M31 = num6;
            result.M32 = num7;
            result.M33 = num8;
        }
        #endregion

        /// <summary>
        /// Matrices are added.
        /// </summary>
        /// <param name="matrix1">The first matrix.</param>
        /// <param name="matrix2">The second matrix.</param>
        /// <returns>The sum of both matrices.</returns>

        #region public static JMatrix Add(JMatrix matrix1, JMatrix matrix2)
        public static Matrix3X3 Add(Matrix3X3 matrix1, Matrix3X3 matrix2)
        {
            Matrix3X3 result;
            Matrix3X3.Add(ref matrix1, ref matrix2, out result);
            return result;
        }

        /// <summary>
        /// Matrices are added.
        /// </summary>
        /// <param name="matrix1">The first matrix.</param>
        /// <param name="matrix2">The second matrix.</param>
        /// <param name="result">The sum of both matrices.</param>
        public static void Add(ref Matrix3X3 matrix1, ref Matrix3X3 matrix2, out Matrix3X3 result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
        }
        #endregion

        /// <summary>
        /// Calculates the inverse of a give matrix.
        /// </summary>
        /// <param name="matrix">The matrix to invert.</param>
        /// <returns>The inverted JMatrix.</returns>

        #region public static JMatrix Inverse(JMatrix matrix)
        public static Matrix3X3 Inverse(Matrix3X3 matrix)
        {
            Matrix3X3 result;
            Matrix3X3.Inverse(ref matrix, out result);
            return result;
        }

        public Fixed64 Determinant()
        {
            return M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 - M31 * M22 * M13 - M32 * M23 * M11 - M33 * M21 * M12;
        }

        public static void Invert(ref Matrix3X3 matrix, out Matrix3X3 result)
        {
            Fixed64 determinantInverse = 1 / matrix.Determinant();
            Fixed64 m11 = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32) * determinantInverse;
            Fixed64 m12 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12) * determinantInverse;
            Fixed64 m13 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13) * determinantInverse;

            Fixed64 m21 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33) * determinantInverse;
            Fixed64 m22 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31) * determinantInverse;
            Fixed64 m23 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23) * determinantInverse;

            Fixed64 m31 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31) * determinantInverse;
            Fixed64 m32 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32) * determinantInverse;
            Fixed64 m33 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21) * determinantInverse;

            result.M11 = m11;
            result.M12 = m12;
            result.M13 = m13;

            result.M21 = m21;
            result.M22 = m22;
            result.M23 = m23;

            result.M31 = m31;
            result.M32 = m32;
            result.M33 = m33;
        }

        /// <summary>
        /// Calculates the inverse of a give matrix.
        /// </summary>
        /// <param name="matrix">The matrix to invert.</param>
        /// <param name="result">The inverted JMatrix.</param>
        public static void Inverse(ref Matrix3X3 matrix, out Matrix3X3 result)
        {
            Fixed64 det = 1024 * matrix.M11 * matrix.M22 * matrix.M33 - 1024 * matrix.M11 * matrix.M23 * matrix.M32 - 1024 * matrix.M12 * matrix.M21 * matrix.M33 + 1024 * matrix.M12 * matrix.M23 * matrix.M31 + 1024 * matrix.M13 * matrix.M21 * matrix.M32 - 1024 * matrix.M13 * matrix.M22 * matrix.M31;

            Fixed64 num11 = 1024 * matrix.M22 * matrix.M33 - 1024 * matrix.M23 * matrix.M32;
            Fixed64 num12 = 1024 * matrix.M13 * matrix.M32 - 1024 * matrix.M12 * matrix.M33;
            Fixed64 num13 = 1024 * matrix.M12 * matrix.M23 - 1024 * matrix.M22 * matrix.M13;

            Fixed64 num21 = 1024 * matrix.M23 * matrix.M31 - 1024 * matrix.M33 * matrix.M21;
            Fixed64 num22 = 1024 * matrix.M11 * matrix.M33 - 1024 * matrix.M31 * matrix.M13;
            Fixed64 num23 = 1024 * matrix.M13 * matrix.M21 - 1024 * matrix.M23 * matrix.M11;

            Fixed64 num31 = 1024 * matrix.M21 * matrix.M32 - 1024 * matrix.M31 * matrix.M22;
            Fixed64 num32 = 1024 * matrix.M12 * matrix.M31 - 1024 * matrix.M32 * matrix.M11;
            Fixed64 num33 = 1024 * matrix.M11 * matrix.M22 - 1024 * matrix.M21 * matrix.M12;

            if (det == 0)
            {
                result.M11 = Fixed64.Infinity;
                result.M12 = Fixed64.Infinity;
                result.M13 = Fixed64.Infinity;
                result.M21 = Fixed64.Infinity;
                result.M22 = Fixed64.Infinity;
                result.M23 = Fixed64.Infinity;
                result.M31 = Fixed64.Infinity;
                result.M32 = Fixed64.Infinity;
                result.M33 = Fixed64.Infinity;
            }
            else
            {
                result.M11 = num11 / det;
                result.M12 = num12 / det;
                result.M13 = num13 / det;
                result.M21 = num21 / det;
                result.M22 = num22 / det;
                result.M23 = num23 / det;
                result.M31 = num31 / det;
                result.M32 = num32 / det;
                result.M33 = num33 / det;
            }
        }
        #endregion

        /// <summary>
        /// Multiply a matrix by a scalefactor.
        /// </summary>
        /// <param name="matrix1">The matrix.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>A JMatrix multiplied by the scale factor.</returns>

        #region public static JMatrix Multiply(JMatrix matrix1, FP scaleFactor)
        public static Matrix3X3 Multiply(Matrix3X3 matrix1, Fixed64 scaleFactor)
        {
            Matrix3X3 result;
            Matrix3X3.Multiply(ref matrix1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Multiply a matrix by a scalefactor.
        /// </summary>
        /// <param name="matrix1">The matrix.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">A JMatrix multiplied by the scale factor.</param>
        public static void Multiply(ref Matrix3X3 matrix1, Fixed64 scaleFactor, out Matrix3X3 result)
        {
            Fixed64 num = scaleFactor;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
        }
        #endregion

        /// <summary>
        /// Creates a JMatrix representing an orientation from a quaternion.
        /// </summary>
        /// <param name="quaternion">The quaternion the matrix should be created from.</param>
        /// <returns>JMatrix representing an orientation.</returns>

        #region public static JMatrix CreateFromQuaternion(JQuaternion quaternion)
        public static Matrix3X3 CreateFromLookAt(Vector3D position, Vector3D target)
        {
            LookAt(target - position, Vector3D.Up, out var result);
            return result;
        }

        public static Matrix3X3 LookAt(Vector3D forward, Vector3D upwards)
        {
            Matrix3X3 result;
            LookAt(forward, upwards, out result);

            return result;
        }

        public static void LookAt(Vector3D forward, Vector3D upwards, out Matrix3X3 result)
        {
            Vector3D zaxis = forward;
            zaxis.Normalize();
            Vector3D xaxis = Vector3D.Cross(upwards, zaxis);
            xaxis.Normalize();
            Vector3D yaxis = Vector3D.Cross(zaxis, xaxis);

            result.M11 = xaxis.X;
            result.M21 = yaxis.X;
            result.M31 = zaxis.X;
            result.M12 = xaxis.Y;
            result.M22 = yaxis.Y;
            result.M32 = zaxis.Y;
            result.M13 = xaxis.Z;
            result.M23 = yaxis.Z;
            result.M33 = zaxis.Z;
        }

        public static Matrix3X3 CreateFromQuaternion(Quaternions quaternion)
        {
            Matrix3X3 result;
            Matrix3X3.CreateFromQuaternion(ref quaternion, out result);
            return result;
        }

        /// <summary>
        /// Creates a JMatrix representing an orientation from a quaternion.
        /// </summary>
        /// <param name="quaternion">The quaternion the matrix should be created from.</param>
        /// <param name="result">JMatrix representing an orientation.</param>
        public static void CreateFromQuaternion(ref Quaternions quaternion, out Matrix3X3 result)
        {
            Fixed64 num9 = quaternion.X * quaternion.X;
            Fixed64 num8 = quaternion.Y * quaternion.Y;
            Fixed64 num7 = quaternion.Z * quaternion.Z;
            Fixed64 num6 = quaternion.X * quaternion.Y;
            Fixed64 num5 = quaternion.Z * quaternion.W;
            Fixed64 num4 = quaternion.Z * quaternion.X;
            Fixed64 num3 = quaternion.Y * quaternion.W;
            Fixed64 num2 = quaternion.Y * quaternion.Z;
            Fixed64 num = quaternion.X * quaternion.W;
            result.M11 = Fixed64.One - (2 * (num8 + num7));
            result.M12 = 2 * (num6 + num5);
            result.M13 = 2 * (num4 - num3);
            result.M21 = 2 * (num6 - num5);
            result.M22 = Fixed64.One - (2 * (num7 + num9));
            result.M23 = 2 * (num2 + num);
            result.M31 = 2 * (num4 + num3);
            result.M32 = 2 * (num2 - num);
            result.M33 = Fixed64.One - (2 * (num8 + num9));
        }
        #endregion

        /// <summary>
        /// Creates the transposed matrix.
        /// </summary>
        /// <param name="matrix">The matrix which should be transposed.</param>
        /// <returns>The transposed JMatrix.</returns>

        #region public static JMatrix Transpose(JMatrix matrix)
        public static Matrix3X3 Transpose(Matrix3X3 matrix)
        {
            Matrix3X3 result;
            Matrix3X3.Transpose(ref matrix, out result);
            return result;
        }

        /// <summary>
        /// Creates the transposed matrix.
        /// </summary>
        /// <param name="matrix">The matrix which should be transposed.</param>
        /// <param name="result">The transposed JMatrix.</param>
        public static void Transpose(ref Matrix3X3 matrix, out Matrix3X3 result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
        }
        #endregion

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="value1">The first matrix.</param>
        /// <param name="value2">The second matrix.</param>
        /// <returns>The product of both values.</returns>

        #region public static JMatrix operator *(JMatrix value1,JMatrix value2)
        public static Matrix3X3 operator *(Matrix3X3 value1, Matrix3X3 value2)
        {
            Matrix3X3 result;
            Matrix3X3.Multiply(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// 3x3矩阵所有值取绝对值
        /// </summary>
        public Matrix3X3 Absolute() => new()
        {
            M11 = M11.Abs(),
            M12 = M12.Abs(),
            M13 = M13.Abs(),
            M21 = M21.Abs(),
            M22 = M22.Abs(),
            M23 = M23.Abs(),
            M31 = M31.Abs(),
            M32 = M32.Abs(),
            M33 = M33.Abs(),
        };

        public Fixed64 Trace()
        {
            return this.M11 + this.M22 + this.M33;
        }

        /// <summary>
        /// Adds two matrices.
        /// </summary>
        /// <param name="value1">The first matrix.</param>
        /// <param name="value2">The second matrix.</param>
        /// <returns>The sum of both values.</returns>

        #region public static JMatrix operator +(JMatrix value1, JMatrix value2)
        public static Matrix3X3 operator +(Matrix3X3 value1, Matrix3X3 value2)
        {
            Matrix3X3 result;
            Matrix3X3.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Subtracts two matrices.
        /// </summary>
        /// <param name="value1">The first matrix.</param>
        /// <param name="value2">The second matrix.</param>
        /// <returns>The difference of both values.</returns>

        #region public static JMatrix operator -(JMatrix value1, JMatrix value2)
        public static Matrix3X3 operator -(Matrix3X3 value1, Matrix3X3 value2)
        {
            Matrix3X3 result;
            Matrix3X3.Multiply(ref value2, -Fixed64.One, out value2);
            Matrix3X3.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        #region public static JVector Transform(JVector position, JMatrix matrix)
        /// <summary>
        /// Checks if the length of the vector is zero.
        /// </summary>
        /// <returns>Returns true if the vector is zero, otherwise false.</returns>
        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <returns>The transformed vector.</returns>
        public static Vector3D Transform(Vector3D position, Matrix3X3 matrix)
        {
            Transform(ref position, ref matrix, out var result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <param name="result">The transformed vector.</param>
        public static void Transform(ref Vector3D position, ref Matrix3X3 matrix, out Vector3D result)
        {
            Fixed64 num0 = ((position.X * matrix.M11) + (position.Y * matrix.M21)) + (position.Z * matrix.M31);
            Fixed64 num1 = ((position.X * matrix.M12) + (position.Y * matrix.M22)) + (position.Z * matrix.M32);
            Fixed64 num2 = ((position.X * matrix.M13) + (position.Y * matrix.M23)) + (position.Z * matrix.M33);

            result.X = num0;
            result.Y = num1;
            result.Z = num2;
        }

        /// <summary>
        /// Transforms a vector by the transposed of the given Matrix.
        /// </summary>
        /// <param name="position">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <param name="result">The transformed vector.</param>
        public static void TransposedTransform(ref Vector3D position, ref Matrix3X3 matrix, out Vector3D result)
        {
            Fixed64 num0 = ((position.X * matrix.M11) + (position.Y * matrix.M12)) + (position.Z * matrix.M13);
            Fixed64 num1 = ((position.X * matrix.M21) + (position.Y * matrix.M22)) + (position.Z * matrix.M23);
            Fixed64 num2 = ((position.X * matrix.M31) + (position.Y * matrix.M32)) + (position.Z * matrix.M33);

            result.X = num0;
            result.Y = num1;
            result.Z = num2;
        }
        #endregion

        public static bool operator ==(Matrix3X3 value1, Matrix3X3 value2)
        {
            return value1.M11 == value2.M11 && value1.M12 == value2.M12 && value1.M13 == value2.M13 && value1.M21 == value2.M21 && value1.M22 == value2.M22 && value1.M23 == value2.M23 && value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M33 == value2.M33;
        }

        public static bool operator !=(Matrix3X3 value1, Matrix3X3 value2)
        {
            return value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 || value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 || value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33;
        }

        public bool Equals(Matrix3X3 other)
        {
            throw new NotImplementedException();
        }
        public int CompareTo(Matrix3X3 other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix3X3))
                return false;
            Matrix3X3 other = (Matrix3X3)obj;

            return this.M11 == other.M11 && this.M12 == other.M12 && this.M13 == other.M13 && this.M21 == other.M21 && this.M22 == other.M22 && this.M23 == other.M23 && this.M31 == other.M31 && this.M32 == other.M32 && this.M33 == other.M33;
        }

        public override int GetHashCode()
        {
            return M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^ M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode();
        }

        /// <summary>
        /// Creates a matrix which rotates around the given axis by the given angle.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="result">The resulting rotation matrix</param>

        #region public static void CreateFromAxisAngle(ref JVector axis, FP angle, out JMatrix result)
        public static void CreateFromAxisAngle(ref Vector3D axis, Fixed64 angle, out Matrix3X3 result)
        {
            var x = axis.X;
            var y = axis.Y;
            var z = axis.Z;
            var num2 = Maths.Sin(angle);
            var num = Maths.Cos(angle);
            var num11 = x * x;
            var num10 = y * y;
            var num9 = z * z;
            var num8 = x * y;
            var num7 = x * z;
            var num6 = y * z;

            result.M11 = num11 + num * (Fixed64.One - num11);
            result.M12 = num8 - num * num8 + num2 * z;
            result.M13 = num7 - num * num7 - num2 * y;
            result.M21 = num8 - num * num8 - num2 * z;
            result.M22 = num10 + num * (Fixed64.One - num10);
            result.M23 = num6 - num * num6 + num2 * x;
            result.M31 = num7 - num * num7 + num2 * y;
            result.M32 = num6 - num * num6 - num2 * x;
            result.M33 = num9 + num * (Fixed64.One - num9);
        }

        /// <summary>
        /// Creates a matrix which rotates around the given axis by the given angle.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The resulting rotation matrix</returns>
        public static Matrix3X3 AngleAxis(Fixed64 angle, Vector3D axis)
        {
            Matrix3X3 result;
            CreateFromAxisAngle(ref axis, angle, out result);
            return result;
        }
        #endregion

        public override string ToString()
        {
            return $"{M11.RawValue}|{M12.RawValue}|{M13.RawValue}|{M21.RawValue}|{M22.RawValue}|{M23.RawValue}|{M31.RawValue}|{M32.RawValue}|{M33.RawValue}";
        }
    }
}