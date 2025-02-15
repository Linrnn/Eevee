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
    public struct Vector4D : IEquatable<Vector4D>, IComparable<Vector4D>
    {
        private static Fixed64 ZeroEpsilonSq = Maths.Epsilon;
        internal static Vector4D InternalZero;

        /// <summary>The X component of the vector.</summary>
        public Fixed64 X;
        /// <summary>The Y component of the vector.</summary>
        public Fixed64 Y;
        /// <summary>The Z component of the vector.</summary>
        public Fixed64 Z;
        /// <summary>The W component of the vector.</summary>
        public Fixed64 W;

        #region Static readonly variables
        /// <summary>
        /// A vector with components (0,0,0,0);
        /// </summary>
        public static readonly Vector4D zero;
        /// <summary>
        /// A vector with components (1,1,1,1);
        /// </summary>
        public static readonly Vector4D one;
        /// <summary>
        /// A vector with components 
        /// (FP.MinValue,FP.MinValue,FP.MinValue);
        /// </summary>
        public static readonly Vector4D MinValue;
        /// <summary>
        /// A vector with components 
        /// (FP.MaxValue,FP.MaxValue,FP.MaxValue);
        /// </summary>
        public static readonly Vector4D MaxValue;
        #endregion

        #region Private static constructor
        static Vector4D()
        {
            one = new Vector4D(1, 1, 1, 1);
            zero = new Vector4D(0, 0, 0, 0);
            MinValue = new Vector4D(Fixed64.MinValue);
            MaxValue = new Vector4D(Fixed64.MaxValue);
            InternalZero = zero;
        }
        #endregion

        public static Vector4D Abs(Vector4D other) => new(other.X.Abs(), other.Y.Abs(), other.Z.Abs(), other.Z.Abs());

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
        private Fixed64 GetSqrMagnitude() => X * X + Y * Y + Z * Z + W * W;

        public static Vector4D ClampMagnitude(Vector4D vector, Fixed64 maxLength) => Normalize(vector) * maxLength;

        /// <summary>
        /// Gets a normalized version of the vector.
        /// </summary>
        /// <returns>Returns a normalized version of the vector.</returns>
        public Vector4D normalized
        {
            get
            {
                var result = this;
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
        /// <param name="w">The W component of the vector.</param>
        public Vector4D(int x, int y, int z, int w)
        {
            this.X = (Fixed64)x;
            this.Y = (Fixed64)y;
            this.Z = (Fixed64)z;
            this.W = (Fixed64)w;
        }

        public Vector4D(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Multiplies each component of the vector by the same components of the provided vector.
        /// </summary>
        public void Scale(Vector4D other)
        {
            this.X = X * other.X;
            this.Y = Y * other.Y;
            this.Z = Z * other.Z;
            this.W = W * other.W;
        }

        /// <summary>
        /// Sets all vector component to specific values.
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        /// <param name="w">The W component of the vector.</param>
        public void Set(Fixed64 x, Fixed64 y, Fixed64 z, Fixed64 w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Constructor initializing a new instance of the structure
        /// </summary>
        /// <param name="xyz">All components of the vector are set to xyz</param>
        public Vector4D(Fixed64 xyzw)
        {
            this.X = xyzw;
            this.Y = xyzw;
            this.Z = xyzw;
            this.W = xyzw;
        }

        public static Vector4D Lerp(Vector4D from, Vector4D to, Fixed64 percent)
        {
            return from + (to - from) * percent;
        }

        /// <summary>
        /// Builds a string from the JVector.
        /// </summary>
        /// <returns>A string containing all three components.</returns>

        #region public override string ToString()
        public override string ToString() => $"({X:f1}, {Y:f1}, {Z:f1}, {W:f1})";
        #endregion

        public bool Equals(Vector4D other)
        {
            throw new NotImplementedException();
        }
        public int CompareTo(Vector4D other)
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
            if (!(obj is Vector4D))
                return false;
            Vector4D other = (Vector4D)obj;

            return (((X == other.X) && (Y == other.Y)) && (Z == other.Z) && (W == other.W));
        }
        #endregion

        /// <summary>
        /// Multiplies each component of the vector by the same components of the provided vector.
        /// </summary>
        public static Vector4D Scale(Vector4D vecA, Vector4D vecB)
        {
            Vector4D result;
            result.X = vecA.X * vecB.X;
            result.Y = vecA.Y * vecB.Y;
            result.Z = vecA.Z * vecB.Z;
            result.W = vecA.W * vecB.W;

            return result;
        }

        /// <summary>
        /// Tests if two JVector are equal.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Returns true if both values are equal, otherwise false.</returns>

        #region public static bool operator ==(JVector value1, JVector value2)
        public static bool operator ==(Vector4D value1, Vector4D value2)
        {
            return (((value1.X == value2.X) && (value1.Y == value2.Y)) && (value1.Z == value2.Z) && (value1.W == value2.W));
        }
        #endregion

        /// <summary>
        /// Tests if two JVector are not equal.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Returns false if both values are equal, otherwise true.</returns>

        #region public static bool operator !=(JVector value1, JVector value2)
        public static bool operator !=(Vector4D value1, Vector4D value2)
        {
            if ((value1.X == value2.X) && (value1.Y == value2.Y) && (value1.Z == value2.Z))
            {
                return (value1.W != value2.W);
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
        public static Vector4D Min(Vector4D value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Min(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Gets a vector with the minimum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="result">A vector with the minimum x,y and z values of both vectors.</param>
        public static void Min(ref Vector4D value1, ref Vector4D value2, out Vector4D result)
        {
            result.X = (value1.X < value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z < value2.Z) ? value1.Z : value2.Z;
            result.W = (value1.W < value2.W) ? value1.W : value2.W;
        }
        #endregion

        /// <summary>
        /// Gets a vector with the maximum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>A vector with the maximum x,y and z values of both vectors.</returns>

        #region public static JVector Max(JVector value1, JVector value2)
        public static Vector4D Max(Vector4D value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Max(ref value1, ref value2, out result);
            return result;
        }

        public static Fixed64 Distance(Vector4D v1, Vector4D v2) => (v1 - v2).magnitude;

        /// <summary>
        /// Gets a vector with the maximum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="result">A vector with the maximum x,y and z values of both vectors.</param>
        public static void Max(ref Vector4D value1, ref Vector4D value2, out Vector4D result)
        {
            result.X = (value1.X > value2.X) ? value1.X : value2.X;
            result.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            result.Z = (value1.Z > value2.Z) ? value1.Z : value2.Z;
            result.W = (value1.W > value2.W) ? value1.W : value2.W;
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
            W = Fixed64.Zero;
        }
        #endregion

        /// <summary>
        /// Checks if the length of the vector is zero.
        /// </summary>
        /// <returns>Returns true if the vector is zero, otherwise false.</returns>

        #region public bool IsZero()
        public bool IsZero()
        {
            return (this.sqrMagnitude == Fixed64.Zero);
        }

        /// <summary>
        /// Checks if the length of the vector is nearly zero.
        /// </summary>
        /// <returns>Returns true if the vector is nearly zero, otherwise false.</returns>
        public bool IsNearlyZero()
        {
            return (this.sqrMagnitude < ZeroEpsilonSq);
        }
        #endregion

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <returns>The transformed vector.</returns>

        #region public static JVector Transform(JVector position, JMatrix matrix)
        public static Vector4D Transform(Vector4D position, Matrix4X4 matrix)
        {
            Vector4D result;
            Vector4D.Transform(ref position, ref matrix, out result);
            return result;
        }

        public static Vector4D Transform(Vector3D position, Matrix4X4 matrix)
        {
            Vector4D result;
            Vector4D.Transform(ref position, ref matrix, out result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <param name="result">The transformed vector.</param>
        public static void Transform(ref Vector3D vector, ref Matrix4X4 matrix, out Vector4D result)
        {
            result.X = vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13 + matrix.M14;
            result.Y = vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23 + matrix.M24;
            result.Z = vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33 + matrix.M34;
            result.W = vector.X * matrix.M41 + vector.Y * matrix.M42 + vector.Z * matrix.M43 + matrix.M44;
        }

        public static void Transform(ref Vector4D vector, ref Matrix4X4 matrix, out Vector4D result)
        {
            result.X = vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13 + vector.W * matrix.M14;
            result.Y = vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23 + vector.W * matrix.M24;
            result.Z = vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33 + vector.W * matrix.M34;
            result.W = vector.X * matrix.M41 + vector.Y * matrix.M42 + vector.Z * matrix.M43 + vector.W * matrix.M44;
        }
        #endregion

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>Returns the dot product of both vectors.</returns>

        #region public static FP Dot(JVector vector1, JVector vector2)
        public static Fixed64 Dot(Vector4D vector1, Vector4D vector2)
        {
            return Vector4D.Dot(ref vector1, ref vector2);
        }

        /// <summary>
        /// Calculates the dot product of both vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>Returns the dot product of both vectors.</returns>
        public static Fixed64 Dot(ref Vector4D vector1, ref Vector4D vector2)
        {
            return ((vector1.X * vector2.X) + (vector1.Y * vector2.Y)) + (vector1.Z * vector2.Z) + (vector1.W * vector2.W);
        }
        #endregion

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The sum of both vectors.</returns>

        #region public static void Add(JVector value1, JVector value2)
        public static Vector4D Add(Vector4D value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Add(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Adds to vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The sum of both vectors.</param>
        public static void Add(ref Vector4D value1, ref Vector4D value2, out Vector4D result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            result.W = value1.W + value2.W;
        }
        #endregion

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        public static Vector4D Divide(Vector4D value1, Fixed64 scaleFactor)
        {
            Vector4D result;
            Vector4D.Divide(ref value1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">Returns the scaled vector.</param>
        public static void Divide(ref Vector4D value1, Fixed64 scaleFactor, out Vector4D result)
        {
            result.X = value1.X / scaleFactor;
            result.Y = value1.Y / scaleFactor;
            result.Z = value1.Z / scaleFactor;
            result.W = value1.W / scaleFactor;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The difference of both vectors.</returns>

        #region public static JVector Subtract(JVector value1, JVector value2)
        public static Vector4D Subtract(Vector4D value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Subtract(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Subtracts to vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The difference of both vectors.</param>
        public static void Subtract(ref Vector4D value1, ref Vector4D value2, out Vector4D result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            result.W = value1.W - value2.W;
        }
        #endregion

        /// <summary>
        /// Gets the hashcode of the vector.
        /// </summary>
        /// <returns>Returns the hashcode of the vector.</returns>

        #region public override int GetHashCode()
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
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
            this.W = -this.W;
        }

        /// <summary>
        /// Inverses the direction of a vector.
        /// </summary>
        /// <param name="value">The vector to inverse.</param>
        /// <returns>The negated vector.</returns>
        public static Vector4D Negate(Vector4D value)
        {
            Vector4D result;
            Vector4D.Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Inverses the direction of a vector.
        /// </summary>
        /// <param name="value">The vector to inverse.</param>
        /// <param name="result">The negated vector.</param>
        public static void Negate(ref Vector4D value, out Vector4D result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
        }
        #endregion

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="value">The vector which should be normalized.</param>
        /// <returns>A normalized vector.</returns>

        #region public static JVector Normalize(JVector value)
        public static Vector4D Normalize(Vector4D value)
        {
            Normalize(ref value, out var result);
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
            W *= num;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="value">The vector which should be normalized.</param>
        /// <param name="result">A normalized vector.</param>
        public static void Normalize(ref Vector4D value, out Vector4D result)
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
        public static void Swap(ref Vector4D vector1, ref Vector4D vector2)
        {
            var temp = vector1.X;
            vector1.X = vector2.X;
            vector2.X = temp;

            temp = vector1.Y;
            vector1.Y = vector2.Y;
            vector2.Y = temp;

            temp = vector1.Z;
            vector1.Z = vector2.Z;
            vector2.Z = temp;

            temp = vector1.W;
            vector1.W = vector2.W;
            vector2.W = temp;
        }
        #endregion

        /// <summary>
        /// Multiply a vector with a factor.
        /// </summary>
        /// <param name="value1">The vector to multiply.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the multiplied vector.</returns>

        #region public static JVector Multiply(JVector value1, FP scaleFactor)
        public static Vector4D Multiply(Vector4D value1, Fixed64 scaleFactor)
        {
            Vector4D result;
            Vector4D.Multiply(ref value1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Multiply a vector with a factor.
        /// </summary>
        /// <param name="value1">The vector to multiply.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">Returns the multiplied vector.</param>
        public static void Multiply(ref Vector4D value1, Fixed64 scaleFactor, out Vector4D result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            result.W = value1.W * scaleFactor;
        }
        #endregion

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>Returns the dot product of both.</returns>

        #region public static FP operator *(JVector value1, JVector value2)
        public static Fixed64 operator *(Vector4D value1, Vector4D value2)
        {
            return Vector4D.Dot(ref value1, ref value2);
        }
        #endregion

        /// <summary>
        /// Multiplies a vector by a scale factor.
        /// </summary>
        /// <param name="value1">The vector to scale.</param>
        /// <param name="value2">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>

        #region public static JVector operator *(JVector value1, FP value2)
        public static Vector4D operator *(Vector4D value1, Fixed64 value2)
        {
            Vector4D result;
            Vector4D.Multiply(ref value1, value2, out result);
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
        public static Vector4D operator *(Fixed64 value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Multiply(ref value2, value1, out result);
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
        public static Vector4D operator -(Vector4D value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Subtract(ref value1, ref value2, out result);
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
        public static Vector4D operator +(Vector4D value1, Vector4D value2)
        {
            Vector4D result;
            Vector4D.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        public static Vector4D operator /(Vector4D value1, Fixed64 value2)
        {
            Vector4D result;
            Vector4D.Divide(ref value1, value2, out result);
            return result;
        }

        public Vector2D ToTSVector2()
        {
            return new Vector2D(this.X, this.Y);
        }

        public Vector3D ToTSVector()
        {
            return new Vector3D(this.X, this.Y, this.Z);
        }
    }
}