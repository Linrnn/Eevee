/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
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
    /// A Quaternion representing an orientation.
    /// </summary>
    [Serializable]
    public struct Quaternions : IEquatable<Quaternions>, IComparable<Quaternions>
    {
        /// <summary>The X component of the quaternion.</summary>
        public Fixed64 X;
        /// <summary>The Y component of the quaternion.</summary>
        public Fixed64 Y;
        /// <summary>The Z component of the quaternion.</summary>
        public Fixed64 Z;
        /// <summary>The W component of the quaternion.</summary>
        public Fixed64 W;

        public static readonly Quaternions identity;

        static Quaternions()
        {
            identity = new Quaternions(0, 0, 0, 1);
        }

        /// <summary>
        /// Initializes a new instance of the JQuaternion structure.
        /// </summary>
        /// <param name="x">The X component of the quaternion.</param>
        /// <param name="y">The Y component of the quaternion.</param>
        /// <param name="z">The Z component of the quaternion.</param>
        /// <param name="w">The W component of the quaternion.</param>
        public Quaternions(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public void Set(Fixed64 new_x, Fixed64 new_y, Fixed64 new_z, Fixed64 new_w)
        {
            this.X = new_x;
            this.Y = new_y;
            this.Z = new_z;
            this.W = new_w;
        }

        public void SetFromToRotation(Vector3D fromDirection, Vector3D toDirection)
        {
            Quaternions targetRotation = Quaternions.FromToRotation(fromDirection, toDirection);
            this.Set(targetRotation.X, targetRotation.Y, targetRotation.Z, targetRotation.W);
        }

        public Vector3D eulerAngles
        {
            get
            {
                Vector3D result = new Vector3D();

                Fixed64 ysqr = Y * Y;
                Fixed64 t0 = -2.0f * (ysqr + Z * Z) + 1.0f;
                Fixed64 t1 = +2.0f * (X * Y - W * Z);
                Fixed64 t2 = -2.0f * (X * Z + W * Y);
                Fixed64 t3 = +2.0f * (Y * Z - W * X);
                Fixed64 t4 = -2.0f * (X * X + ysqr) + 1.0f;

                t2 = t2 > 1.0f ? 1.0f : t2;
                t2 = t2 < -1.0f ? -1.0f : t2;

                result.X = Fixed64.Atan2(t3, t4) * Maths.Rad2Deg;
                result.Y = Fixed64.Asin(t2) * Maths.Rad2Deg;
                result.Z = Fixed64.Atan2(t1, t0) * Maths.Rad2Deg;

                return result * -1;
            }
        }

        public static Fixed64 Angle(Quaternions a, Quaternions b)
        {
            Quaternions aInv = Quaternions.Inverse(a);
            Quaternions f = b * aInv;

            Fixed64 angle = Fixed64.Acos(f.W) * 2 * Maths.Rad2Deg;

            if (angle > 180)
            {
                angle = 360 - angle;
            }

            return angle;
        }

        /// <summary>
        /// Quaternions are added.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <returns>The sum of both quaternions.</returns>

        #region public static JQuaternion Add(JQuaternion quaternion1, JQuaternion quaternion2)
        public static Quaternions Add(Quaternions quaternion1, Quaternions quaternion2)
        {
            Quaternions result;
            Quaternions.Add(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        public static Quaternions LookRotation(Vector3D forward)
        {
            return CreateFromMatrix(Matrix3X3.LookAt(forward, Vector3D.up));
        }

        public static Quaternions LookRotation(Vector3D forward, Vector3D upwards)
        {
            return CreateFromMatrix(Matrix3X3.LookAt(forward, upwards));
        }

        public static Quaternions Slerp(Quaternions from, Quaternions to, Fixed64 t)
        {
            t = Maths.Clamp(t, 0, 1);

            Fixed64 dot = Dot(from, to);

            if (dot < 0.0f)
            {
                to = Multiply(to, -1);
                dot = -dot;
            }

            Fixed64 halfTheta = Fixed64.Acos(dot);

            return Multiply(Multiply(from, Maths.SinRad((1 - t) * halfTheta)) + Multiply(to, Maths.SinRad(t * halfTheta)), 1 / Maths.SinRad(halfTheta));
        }

        public static Quaternions RotateTowards(Quaternions from, Quaternions to, Fixed64 maxDegreesDelta)
        {
            Fixed64 dot = Dot(from, to);

            if (dot < 0.0f)
            {
                to = Multiply(to, -1);
                dot = -dot;
            }

            Fixed64 halfTheta = Fixed64.Acos(dot);
            Fixed64 theta = halfTheta * 2;

            maxDegreesDelta *= Maths.Deg2Rad;

            if (maxDegreesDelta >= theta)
            {
                return to;
            }

            maxDegreesDelta /= theta;

            return Multiply(Multiply(from, Maths.SinRad((1 - maxDegreesDelta) * halfTheta)) + Multiply(to, Maths.SinRad(maxDegreesDelta * halfTheta)), 1 / Maths.SinRad(halfTheta));
        }

        public static Quaternions Euler(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            x *= Maths.Deg2Rad;
            y *= Maths.Deg2Rad;
            z *= Maths.Deg2Rad;

            Quaternions rotation;
            Quaternions.CreateFromYawPitchRoll(y, x, z, out rotation);

            return rotation;
        }

        public static Quaternions Euler(Vector3D eulerAngles)
        {
            return Euler(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        }

        public static Quaternions AngleAxis(Fixed64 angle, Vector3D axis)
        {
            axis *= Maths.Deg2Rad;
            axis.Normalize();

            var halfAngle = angle * Maths.Deg2Rad * Fixed64.Half;
            var sin = Maths.SinRad(halfAngle);

            Quaternions rotation;
            rotation.X = axis.X * sin;
            rotation.Y = axis.Y * sin;
            rotation.Z = axis.Z * sin;
            rotation.W = Maths.CosRad(halfAngle);
            return rotation;
        }

        public static void CreateFromYawPitchRoll(Fixed64 yaw, Fixed64 pitch, Fixed64 roll, out Quaternions result)
        {
            var num9 = roll * Fixed64.Half;
            var num6 = Maths.SinRad(num9);
            var num5 = Maths.CosRad(num9);
            var num8 = pitch * Fixed64.Half;
            var num4 = Maths.SinRad(num8);
            var num3 = Maths.CosRad(num8);
            var num7 = yaw * Fixed64.Half;
            var num2 = Maths.SinRad(num7);
            var num = Maths.CosRad(num7);

            result.X = num * num4 * num5 + num2 * num3 * num6;
            result.Y = num2 * num3 * num5 - num * num4 * num6;
            result.Z = num * num3 * num6 - num2 * num4 * num5;
            result.W = num * num3 * num5 + num2 * num4 * num6;
        }

        /// <summary>
        /// Quaternions are added.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <param name="result">The sum of both quaternions.</param>
        public static void Add(ref Quaternions quaternion1, ref Quaternions quaternion2, out Quaternions result)
        {
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
        }
        #endregion

        public static Quaternions Conjugate(Quaternions value)
        {
            Quaternions quaternion;
            quaternion.X = -value.X;
            quaternion.Y = -value.Y;
            quaternion.Z = -value.Z;
            quaternion.W = value.W;
            return quaternion;
        }

        public static Fixed64 Dot(Quaternions a, Quaternions b)
        {
            return a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Quaternions Inverse(Quaternions rotation)
        {
            var invNorm = (rotation.X * rotation.X + rotation.Y * rotation.Y + rotation.Z * rotation.Z + rotation.W * rotation.W).Reciprocal;
            return Multiply(Conjugate(rotation), invNorm);
        }

        public static Quaternions FromToRotation(Vector3D fromVector, Vector3D toVector)
        {
            var w = Vector3D.Cross(fromVector, toVector);
            Quaternions q = new Quaternions(w.X, w.Y, w.Z, Vector3D.Dot(fromVector, toVector));
            q.W += (fromVector.sqrMagnitude * toVector.sqrMagnitude).Sqrt;
            q.Normalize();

            return q;
        }

        public static Quaternions Lerp(Quaternions a, Quaternions b, Fixed64 t)
        {
            t = Maths.Clamp(t, Fixed64.Zero, Fixed64.One);

            return LerpUnclamped(a, b, t);
        }

        public static Quaternions LerpUnclamped(Quaternions a, Quaternions b, Fixed64 t)
        {
            Quaternions result = Quaternions.Multiply(a, (1 - t)) + Quaternions.Multiply(b, t);
            result.Normalize();

            return result;
        }

        /// <summary>
        /// Quaternions are subtracted.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <returns>The difference of both quaternions.</returns>

        #region public static JQuaternion Subtract(JQuaternion quaternion1, JQuaternion quaternion2)
        public static Quaternions Subtract(Quaternions quaternion1, Quaternions quaternion2)
        {
            Quaternions result;
            Quaternions.Subtract(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        /// <summary>
        /// Quaternions are subtracted.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <param name="result">The difference of both quaternions.</param>
        public static void Subtract(ref Quaternions quaternion1, ref Quaternions quaternion2, out Quaternions result)
        {
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
        }
        #endregion

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <returns>The product of both quaternions.</returns>

        #region public static JQuaternion Multiply(JQuaternion quaternion1, JQuaternion quaternion2)
        public static Quaternions Multiply(Quaternions quaternion1, Quaternions quaternion2)
        {
            Quaternions result;
            Quaternions.Multiply(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <param name="result">The product of both quaternions.</param>
        public static void Multiply(ref Quaternions quaternion1, ref Quaternions quaternion2, out Quaternions result)
        {
            Fixed64 x = quaternion1.X;
            Fixed64 y = quaternion1.Y;
            Fixed64 z = quaternion1.Z;
            Fixed64 w = quaternion1.W;
            Fixed64 num4 = quaternion2.X;
            Fixed64 num3 = quaternion2.Y;
            Fixed64 num2 = quaternion2.Z;
            Fixed64 num = quaternion2.W;
            Fixed64 num12 = (y * num2) - (z * num3);
            Fixed64 num11 = (z * num4) - (x * num2);
            Fixed64 num10 = (x * num3) - (y * num4);
            Fixed64 num9 = ((x * num4) + (y * num3)) + (z * num2);
            result.X = ((x * num) + (num4 * w)) + num12;
            result.Y = ((y * num) + (num3 * w)) + num11;
            result.Z = ((z * num) + (num2 * w)) + num10;
            result.W = (w * num) - num9;
        }
        #endregion

        /// <summary>
        /// Scale a quaternion
        /// </summary>
        /// <param name="quaternion1">The quaternion to scale.</param>
        /// <param name="scaleFactor">Scale factor.</param>
        /// <returns>The scaled quaternion.</returns>

        #region public static JQuaternion Multiply(JQuaternion quaternion1, FP scaleFactor)
        public static Quaternions Multiply(Quaternions quaternion1, Fixed64 scaleFactor)
        {
            Quaternions result;
            Quaternions.Multiply(ref quaternion1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Scale a quaternion
        /// </summary>
        /// <param name="quaternion1">The quaternion to scale.</param>
        /// <param name="scaleFactor">Scale factor.</param>
        /// <param name="result">The scaled quaternion.</param>
        public static void Multiply(ref Quaternions quaternion1, Fixed64 scaleFactor, out Quaternions result)
        {
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
        }
        #endregion

        /// <summary>
        /// Sets the length of the quaternion to one.
        /// </summary>

        #region public void Normalize()
        public void Normalize()
        {
            var num2 = X * X + Y * Y + Z * Z + W * W;
            var num = num2.Sqrt.Reciprocal;

            X *= num;
            Y *= num;
            Z *= num;
            W *= num;
        }
        #endregion

        /// <summary>
        /// Creates a quaternion from a matrix.
        /// </summary>
        /// <param name="matrix">A matrix representing an orientation.</param>
        /// <returns>JQuaternion representing an orientation.</returns>

        #region public static JQuaternion CreateFromMatrix(JMatrix matrix)
        public static Quaternions CreateFromMatrix(Matrix3X3 matrix)
        {
            CreateFromMatrix(ref matrix, out var result);
            return result;
        }

        /// <summary>
        /// Creates a quaternion from a matrix.
        /// </summary>
        /// <param name="matrix">A matrix representing an orientation.</param>
        /// <param name="result">JQuaternion representing an orientation.</param>
        public static void CreateFromMatrix(ref Matrix3X3 matrix, out Quaternions result)
        {
            Fixed64 num8 = (matrix.M11 + matrix.M22) + matrix.M33;
            if (num8 > Fixed64.Zero)
            {
                var num = (num8 + Fixed64.One).Sqrt;
                result.W = num * Fixed64.Half;
                num = Fixed64.Half / num;
                result.X = (matrix.M23 - matrix.M32) * num;
                result.Y = (matrix.M31 - matrix.M13) * num;
                result.Z = (matrix.M12 - matrix.M21) * num;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                var num7 = (Fixed64.One + matrix.M11 - matrix.M22 - matrix.M33).Sqrt;
                var num4 = Fixed64.Half / num7;
                result.X = Fixed64.Half * num7;
                result.Y = (matrix.M12 + matrix.M21) * num4;
                result.Z = (matrix.M13 + matrix.M31) * num4;
                result.W = (matrix.M23 - matrix.M32) * num4;
            }
            else if (matrix.M22 > matrix.M33)
            {
                var num6 = (Fixed64.One + matrix.M22 - matrix.M11 - matrix.M33).Sqrt;
                var num3 = Fixed64.Half / num6;
                result.X = (matrix.M21 + matrix.M12) * num3;
                result.Y = Fixed64.Half * num6;
                result.Z = (matrix.M32 + matrix.M23) * num3;
                result.W = (matrix.M31 - matrix.M13) * num3;
            }
            else
            {
                var num5 = (Fixed64.One + matrix.M33 - matrix.M11 - matrix.M22).Sqrt;
                var num2 = Fixed64.Half / num5;
                result.X = (matrix.M31 + matrix.M13) * num2;
                result.Y = (matrix.M32 + matrix.M23) * num2;
                result.Z = Fixed64.Half * num5;
                result.W = (matrix.M12 - matrix.M21) * num2;
            }
        }
        #endregion

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="value1">The first quaternion.</param>
        /// <param name="value2">The second quaternion.</param>
        /// <returns>The product of both quaternions.</returns>

        #region public static FP operator *(JQuaternion value1, JQuaternion value2)
        public static Quaternions operator *(Quaternions value1, Quaternions value2)
        {
            Quaternions result;
            Quaternions.Multiply(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Add two quaternions.
        /// </summary>
        /// <param name="value1">The first quaternion.</param>
        /// <param name="value2">The second quaternion.</param>
        /// <returns>The sum of both quaternions.</returns>

        #region public static FP operator +(JQuaternion value1, JQuaternion value2)
        public static Quaternions operator +(Quaternions value1, Quaternions value2)
        {
            Quaternions result;
            Quaternions.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Subtract two quaternions.
        /// </summary>
        /// <param name="value1">The first quaternion.</param>
        /// <param name="value2">The second quaternion.</param>
        /// <returns>The difference of both quaternions.</returns>

        #region public static FP operator -(JQuaternion value1, JQuaternion value2)
        public static Quaternions operator -(Quaternions value1, Quaternions value2)
        {
            Quaternions result;
            Quaternions.Subtract(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /**
         *  @brief Rotates a {@link TSVector} by the {@link TSQuanternion}.
         **/
        public static Vector3D operator *(Quaternions quat, Vector3D vec)
        {
            Fixed64 num = quat.X * 2f;
            Fixed64 num2 = quat.Y * 2f;
            Fixed64 num3 = quat.Z * 2f;
            Fixed64 num4 = quat.X * num;
            Fixed64 num5 = quat.Y * num2;
            Fixed64 num6 = quat.Z * num3;
            Fixed64 num7 = quat.X * num2;
            Fixed64 num8 = quat.X * num3;
            Fixed64 num9 = quat.Y * num3;
            Fixed64 num10 = quat.W * num;
            Fixed64 num11 = quat.W * num2;
            Fixed64 num12 = quat.W * num3;

            Vector3D result;
            result.X = (1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z;
            result.Y = (num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z;
            result.Z = (num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z;

            return result;
        }

        #region operator == And !=
        public static bool operator ==(Quaternions value1, Quaternions value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z && value1.W == value2.W;
        }

        public static bool operator !=(Quaternions value1, Quaternions value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y || value1.Z != value2.Z || value1.W != value2.W;
        }
        #endregion

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public bool Equals(Quaternions other)
        {
            throw new NotImplementedException();
        }
        public int CompareTo(Quaternions other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("({0:f1}, {1:f1}, {2:f1}, {3:f1})", X.AsFloat(), Y.AsFloat(), Z.AsFloat(), W.AsFloat());
        }
    }
}