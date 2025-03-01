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
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// A vector structure.
    /// </summary>
    [Serializable]
    public struct Vector3D : IEquatable<Vector3D>, IComparable<Vector3D>
    {
        internal static Vector3D InternalZero;
        internal static Vector3D Arbitrary;

        /// <summary>The X component of the vector.</summary>
        public Fixed64 X;
        /// <summary>The Y component of the vector.</summary>
        public Fixed64 Y;
        /// <summary>The Z component of the vector.</summary>
        public Fixed64 Z;

        #region Static readonly variables
        /// <summary>
        /// A vector with components (0,0,0);
        /// </summary>
        public static readonly Vector3D zero;
        /// <summary>
        /// A vector with components (-1,0,0);
        /// </summary>
        public static readonly Vector3D left;
        /// <summary>
        /// A vector with components (1,0,0);
        /// </summary>
        public static readonly Vector3D right;
        /// <summary>
        /// A vector with components (0,1,0);
        /// </summary>
        public static readonly Vector3D up;
        /// <summary>
        /// A vector with components (0,-1,0);
        /// </summary>
        public static readonly Vector3D down;
        /// <summary>
        /// A vector with components (0,0,-1);
        /// </summary>
        public static readonly Vector3D back;
        /// <summary>
        /// A vector with components (0,0,1);
        /// </summary>
        public static readonly Vector3D forward;
        /// <summary>
        /// A vector with components (1,1,1);
        /// </summary>
        public static readonly Vector3D one;
        /// <summary>
        /// A vector with components 
        /// (FP.MinValue,FP.MinValue,FP.MinValue);
        /// </summary>
        public static readonly Vector3D MinValue;
        /// <summary>
        /// A vector with components 
        /// (FP.MaxValue,FP.MaxValue,FP.MaxValue);
        /// </summary>
        public static readonly Vector3D MaxValue;
        #endregion

        #region Private static constructor
        static Vector3D()
        {
            one = new Vector3D(1, 1, 1);
            zero = new Vector3D(0, 0, 0);
            left = new Vector3D(-1, 0, 0);
            right = new Vector3D(1, 0, 0);
            up = new Vector3D(0, 1, 0);
            down = new Vector3D(0, -1, 0);
            back = new Vector3D(0, 0, -1);
            forward = new Vector3D(0, 0, 1);
            MinValue = new Vector3D(Fixed64.MinValue);
            MaxValue = new Vector3D(Fixed64.MaxValue);
            Arbitrary = new Vector3D(1, 1, 1);
            InternalZero = zero;
        }
        #endregion

        public static Vector3D Abs(Vector3D other) => new(other.X.Abs(), other.Y.Abs(), other.Z.Abs());

        /// <summary>
        /// Gets the squared length of the vector.
        /// </summary>
        /// <returns>Returns the squared length of the vector.</returns>
        public Fixed64 sqrMagnitude => GetSqrMagnitude();

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <returns>Returns the length of the vector.</returns>
        public Fixed64 magnitude => GetSqrMagnitude().Sqrt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Fixed64 GetSqrMagnitude() => X * X + Y * Y + Z * Z;

        public static Vector3D ClampMagnitude(Vector3D vector, Fixed64 maxLength)
        {
            return Normalize(vector) * maxLength;
        }

        /// <summary>
        /// Gets a normalized version of the vector.
        /// </summary>
        /// <returns>Returns a normalized version of the vector.</returns>
        public Vector3D normalized
        {
            get
            {
                Vector3D result = new Vector3D(this.X, this.Y, this.Z);
                result.Normalize();

                return result;
            }
        }

        /// <summary>
        /// Constructor initializing a new instance of the structure
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        public Vector3D(int x, int y, int z)
        {
            this.X = (Fixed64)x;
            this.Y = (Fixed64)y;
            this.Z = (Fixed64)z;
        }

        public Vector3D(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Multiplies each component of the vector by the same components of the provided vector.
        /// </summary>
        public void Scale(Vector3D other)
        {
            this.X = X * other.X;
            this.Y = Y * other.Y;
            this.Z = Z * other.Z;
        }

        /// <summary>
        /// Sets all vector component to specific values.
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        public void Set(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Constructor initializing a new instance of the structure
        /// </summary>
        /// <param name="xyz">All components of the vector are set to xyz</param>
        public Vector3D(Fixed64 xyz)
        {
            this.X = xyz;
            this.Y = xyz;
            this.Z = xyz;
        }

        public static Vector3D Lerp(Vector3D from, Vector3D to, Fixed64 percent)
        {
            return from + (to - from) * percent;
        }

        /// <summary>
        /// Builds a string from the JVector.
        /// </summary>
        /// <returns>A string containing all three components.</returns>

        #region public override string ToString()
        public override string ToString() => $"({X:f1}, {Y:f1}, {Z:f1})";
        #endregion

        public bool Equals(Vector3D other)
        {
            throw new NotImplementedException();
        }
        public int CompareTo(Vector3D other)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Tests if an object is equal to this vector.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>Returns true if they are euqal, otherwise false.</returns>

        #region public override bool Equals(object obj)
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3D))
                return false;
            Vector3D other = (Vector3D)obj;

            return (((X == other.X) && (Y == other.Y)) && (Z == other.Z));
        }
        #endregion

        /// <summary>
        /// Multiplies each component of the vector by the same components of the provided vector.
        /// </summary>
        public static Vector3D Scale(Vector3D vecA, Vector3D vecB)
        {
            Vector3D result;
            result.X = vecA.X * vecB.X;
            result.Y = vecA.Y * vecB.Y;
            result.Z = vecA.Z * vecB.Z;

            return result;
        }

        /// <summary>
        /// Tests if two JVector are equal.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Returns true if both values are equal, otherwise false.</returns>

        #region public static bool operator ==(JVector value1, JVector value2)
        public static bool operator ==(Vector3D value1, Vector3D value2)
        {
            return (((value1.X == value2.X) && (value1.Y == value2.Y)) && (value1.Z == value2.Z));
        }
        #endregion

        /// <summary>
        /// Tests if two JVector are not equal.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Returns false if both values are equal, otherwise true.</returns>

        #region public static bool operator !=(JVector value1, JVector value2)
        public static bool operator !=(Vector3D value1, Vector3D value2)
        {
            if ((value1.X == value2.X) && (value1.Y == value2.Y))
            {
                return (value1.Z != value2.Z);
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Gets a vector with the minimum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>A vector with the minimum x,y and z values of both vectors.</returns>

        #region public static JVector Min(JVector value1, JVector value2)
        public static Vector3D Min(Vector3D value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Min(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Gets a vector with the minimum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="result">A vector with the minimum x,y and z values of both vectors.</param>
        public static void Min(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
        {
            result.X = (value1.X < value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z < value2.Z) ? value1.Z : value2.Z;
        }
        #endregion

        /// <summary>
        /// Gets a vector with the maximum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>A vector with the maximum x,y and z values of both vectors.</returns>

        #region public static JVector Max(JVector value1, JVector value2)
        public static Vector3D Max(Vector3D value1, Vector3D value2)
        {
            Max(ref value1, ref value2, out var result);
            return result;
        }

        public static Fixed64 Distance(Vector3D v1, Vector3D v2) => (v1 - v2).magnitude;

        /// <summary>
        /// Gets a vector with the maximum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="result">A vector with the maximum x,y and z values of both vectors.</param>
        public static void Max(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
        {
            result.X = value1.X > value2.X ? value1.X : value2.X;
            result.Y = value1.Y > value2.Y ? value1.Y : value2.Y;
            result.Z = value1.Z > value2.Z ? value1.Z : value2.Z;
        }
        #endregion

        /// <summary>
        /// Sets the length of the vector to zero.
        /// </summary>

        #region public void MakeZero()
        public void MakeZero()
        {
            X = Fixed64.Zero;
            Y = Fixed64.Zero;
            Z = Fixed64.Zero;
        }
        #endregion

        /// <summary>
        /// Checks if the length of the vector is zero.
        /// </summary>
        /// <returns>Returns true if the vector is zero, otherwise false.</returns>

        #region public bool IsZero()
        public bool IsZero() => sqrMagnitude.RawValue == 0L;

        /// <summary>
        /// Checks if the length of the vector is nearly zero.
        /// </summary>
        /// <returns>Returns true if the vector is nearly zero, otherwise false.</returns>
        public bool IsNearlyZero() => sqrMagnitude.RawValue <= Const.Epsilon;
        #endregion

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <returns>The transformed vector.</returns>

        #region public static JVector Transform(JVector position, JMatrix matrix)
        public static Vector3D Transform(Vector3D position, Matrix3X3 matrix)
        {
            Vector3D result;
            Vector3D.Transform(ref position, ref matrix, out result);
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

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>Returns the dot product of both vectors.</returns>

        #region public static FP Dot(JVector vector1, JVector vector2)
        public static Fixed64 Dot(Vector3D vector1, Vector3D vector2)
        {
            return Vector3D.Dot(ref vector1, ref vector2);
        }

        /// <summary>
        /// Calculates the dot product of both vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>Returns the dot product of both vectors.</returns>
        public static Fixed64 Dot(ref Vector3D vector1, ref Vector3D vector2)
        {
            return ((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z);
        }
        #endregion

        // Projects a vector onto another vector.
        public static Vector3D Project(Vector3D vector, Vector3D onNormal)
        {
            var sqrtMag = Dot(onNormal, onNormal);
            if (sqrtMag.RawValue <= Const.Epsilon)
                return zero;
            return onNormal * Dot(vector, onNormal) / sqrtMag;
        }

        // Projects a vector onto a plane defined by a normal orthogonal to the plane.
        public static Vector3D ProjectOnPlane(Vector3D vector, Vector3D planeNormal)
        {
            return vector - Project(vector, planeNormal);
        }

        // Returns the angle in degrees between /from/ and /to/. This is always the smallest
        public static Fixed64 Angle(Vector3D from, Vector3D to) => Maths.AcosDeg(Maths.Clamp(Dot(from.normalized, to.normalized), -Fixed64.One, Fixed64.One));

        // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
        // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
        public static Fixed64 SignedAngle(Vector3D from, Vector3D to, Vector3D axis)
        {
            var fromNorm = from.normalized;
            var toNorm = to.normalized;
            var unsignedAngle = Maths.AcosDeg(Maths.Clamp(Dot(fromNorm, toNorm), -Fixed64.One, Fixed64.One));
            int sign = Dot(axis, Cross(fromNorm, toNorm)).Sign();
            return unsignedAngle * sign;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The sum of both vectors.</returns>

        #region public static void Add(JVector value1, JVector value2)
        public static Vector3D Add(Vector3D value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Add(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Adds to vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The sum of both vectors.</param>
        public static void Add(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
        {
            Fixed64 num0 = value1.X + value2.X;
            Fixed64 num1 = value1.Y + value2.Y;
            Fixed64 num2 = value1.Z + value2.Z;

            result.X = num0;
            result.Y = num1;
            result.Z = num2;
        }
        #endregion

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        public static Vector3D Divide(Vector3D value1, Fixed64 scaleFactor)
        {
            Vector3D result;
            Vector3D.Divide(ref value1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">Returns the scaled vector.</param>
        public static void Divide(ref Vector3D value1, Fixed64 scaleFactor, out Vector3D result)
        {
            result.X = value1.X / scaleFactor;
            result.Y = value1.Y / scaleFactor;
            result.Z = value1.Z / scaleFactor;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The difference of both vectors.</returns>

        #region public static JVector Subtract(JVector value1, JVector value2)
        public static Vector3D Subtract(Vector3D value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Subtract(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Subtracts to vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The difference of both vectors.</param>
        public static void Subtract(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
        {
            Fixed64 num0 = value1.X - value2.X;
            Fixed64 num1 = value1.Y - value2.Y;
            Fixed64 num2 = value1.Z - value2.Z;

            result.X = num0;
            result.Y = num1;
            result.Z = num2;
        }
        #endregion

        /// <summary>
        /// The cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product of both vectors.</returns>

        #region public static JVector Cross(JVector vector1, JVector vector2)
        public static Vector3D Cross(Vector3D vector1, Vector3D vector2)
        {
            Vector3D result;
            Vector3D.Cross(ref vector1, ref vector2, out result);
            return result;
        }

        /// <summary>
        /// The cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <param name="result">The cross product of both vectors.</param>
        public static void Cross(ref Vector3D vector1, ref Vector3D vector2, out Vector3D result)
        {
            Fixed64 num3 = (vector1.Y * vector2.Z) - (vector1.Z * vector2.Y);
            Fixed64 num2 = (vector1.Z * vector2.X) - (vector1.X * vector2.Z);
            Fixed64 num = (vector1.X * vector2.Y) - (vector1.Y * vector2.X);
            result.X = num3;
            result.Y = num2;
            result.Z = num;
        }
        #endregion

        /// <summary>
        /// 只计算Y的叉积，用于快速判断两个向量的相对位置
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Fixed64 Cross2D(Vector3D vector1, Vector3D vector2)
        {
            return vector1.Z * vector2.X - vector1.X * vector2.Z;
        }

        /// <summary>
        /// Gets the hashcode of the vector.
        /// </summary>
        /// <returns>Returns the hashcode of the vector.</returns>

        #region public override int GetHashCode()
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
        #endregion

        /// <summary>
        /// Inverses the direction of the vector.
        /// </summary>

        #region public static JVector Negate(JVector value)
        public void Negate()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }

        /// <summary>
        /// Inverses the direction of a vector.
        /// </summary>
        /// <param name="value">The vector to inverse.</param>
        /// <returns>The negated vector.</returns>
        public static Vector3D Negate(Vector3D value)
        {
            Vector3D result;
            Vector3D.Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Inverses the direction of a vector.
        /// </summary>
        /// <param name="value">The vector to inverse.</param>
        /// <param name="result">The negated vector.</param>
        public static void Negate(ref Vector3D value, out Vector3D result)
        {
            Fixed64 num0 = -value.X;
            Fixed64 num1 = -value.Y;
            Fixed64 num2 = -value.Z;

            result.X = num0;
            result.Y = num1;
            result.Z = num2;
        }
        #endregion

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="value">The vector which should be normalized.</param>
        /// <returns>A normalized vector.</returns>

        #region public static JVector Normalize(JVector value)
        public static Vector3D Normalize(Vector3D value)
        {
            Vector3D result;
            Vector3D.Normalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public void Normalize()
        {
            var num = magnitude.Reciprocal();
            X *= num;
            Y *= num;
            Z *= num;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="value">The vector which should be normalized.</param>
        /// <param name="result">A normalized vector.</param>
        public static void Normalize(ref Vector3D value, out Vector3D result)
        {
            result = value;
            result.Normalize();
        }
        #endregion

        #region public static void Swap(ref JVector vector1, ref JVector vector2)
        /// <summary>
        /// Swaps the components of both vectors.
        /// </summary>
        /// <param name="vector1">The first vector to swap with the second.</param>
        /// <param name="vector2">The second vector to swap with the first.</param>
        public static void Swap(ref Vector3D vector1, ref Vector3D vector2)
        {
            Fixed64 temp;

            temp = vector1.X;
            vector1.X = vector2.X;
            vector2.X = temp;

            temp = vector1.Y;
            vector1.Y = vector2.Y;
            vector2.Y = temp;

            temp = vector1.Z;
            vector1.Z = vector2.Z;
            vector2.Z = temp;
        }
        #endregion

        /// <summary>
        /// Multiply a vector with a factor.
        /// </summary>
        /// <param name="value1">The vector to multiply.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the multiplied vector.</returns>

        #region public static JVector Multiply(JVector value1, FP scaleFactor)
        public static Vector3D Multiply(Vector3D value1, Fixed64 scaleFactor)
        {
            Vector3D result;
            Vector3D.Multiply(ref value1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Multiply a vector with a factor.
        /// </summary>
        /// <param name="value1">The vector to multiply.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">Returns the multiplied vector.</param>
        public static void Multiply(ref Vector3D value1, Fixed64 scaleFactor, out Vector3D result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }
        #endregion

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>Returns the cross product of both.</returns>

        #region public static JVector operator %(JVector value1, JVector value2)
        public static Vector3D operator %(Vector3D value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Cross(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>Returns the dot product of both.</returns>

        #region public static FP operator *(JVector value1, JVector value2)
        public static Fixed64 operator *(Vector3D value1, Vector3D value2)
        {
            return Vector3D.Dot(ref value1, ref value2);
        }
        #endregion

        /// <summary>
        /// Multiplies a vector by a scale factor.
        /// </summary>
        /// <param name="value1">The vector to scale.</param>
        /// <param name="value2">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>

        #region public static JVector operator *(JVector value1, FP value2)
        public static Vector3D operator *(Vector3D value1, Fixed64 value2)
        {
            Vector3D result;
            Vector3D.Multiply(ref value1, value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Multiplies a vector by a scale factor.
        /// </summary>
        /// <param name="value2">The vector to scale.</param>
        /// <param name="value1">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>

        #region public static JVector operator *(FP value1, JVector value2)
        public static Vector3D operator *(Fixed64 value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Multiply(ref value2, value1, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The difference of both vectors.</returns>

        #region public static JVector operator -(JVector value1, JVector value2)
        public static Vector3D operator -(Vector3D value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Subtract(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The sum of both vectors.</returns>

        #region public static JVector operator +(JVector value1, JVector value2)
        public static Vector3D operator +(Vector3D value1, Vector3D value2)
        {
            Vector3D result;
            Vector3D.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        public static Vector3D operator /(Vector3D value1, Fixed64 value2)
        {
            Vector3D result;
            Vector3D.Divide(ref value1, value2, out result);
            return result;
        }

        public Vector2D ToVector2()
        {
            return new Vector2D(this.X, this.Y);
        }

        public Vector4D ToVector4()
        {
            return new Vector4D(this.X, this.Y, this.Z, Fixed64.One);
        }
    }
}