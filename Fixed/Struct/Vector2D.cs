#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors
 * Alan McGovern

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;

namespace Eevee.Fixed
{
    [Serializable]
    public struct Vector2D : IEquatable<Vector2D>, IComparable<Vector2D>
    {
        #region Private Fields
        private static Vector2D zeroVector = new Vector2D(0, 0);
        private static Vector2D oneVector = new Vector2D(1, 1);

        private static Vector2D rightVector = new Vector2D(1, 0);
        private static Vector2D leftVector = new Vector2D(-1, 0);

        private static Vector2D upVector = new Vector2D(0, 1);
        private static Vector2D downVector = new Vector2D(0, -1);
        #endregion Private Fields

        #region Public Fields
        public Fixed64 X;
        public Fixed64 Y;
        #endregion Public Fields

        #region Properties
        public static Vector2D zero
        {
            get { return zeroVector; }
        }

        public static Vector2D one
        {
            get { return oneVector; }
        }

        public static Vector2D right
        {
            get { return rightVector; }
        }

        public static Vector2D left
        {
            get { return leftVector; }
        }

        public static Vector2D up
        {
            get { return upVector; }
        }

        public static Vector2D down
        {
            get { return downVector; }
        }
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Constructor foe standard 2D vector.
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/>
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/>
        /// </param>
        public Vector2D(Fixed64 x, Fixed64 y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Constructor for "square" vector.
        /// </summary>
        /// <param name="value">
        /// A <see cref="System.Single"/>
        /// </param>
        public Vector2D(Fixed64 value)
        {
            X = value;
            Y = value;
        }

        public void Set(Fixed64 x, Fixed64 y)
        {
            this.X = x;
            this.Y = y;
        }
        #endregion Constructors

        #region Public Methods
        public static void Reflect(ref Vector2D vector, ref Vector2D normal, out Vector2D result)
        {
            Fixed64 dot = Dot(vector, normal);
            result.X = vector.X - ((2f * dot) * normal.X);
            result.Y = vector.Y - ((2f * dot) * normal.Y);
        }

        public static Vector2D Reflect(Vector2D vector, Vector2D normal)
        {
            Vector2D result;
            Reflect(ref vector, ref normal, out result);
            return result;
        }

        public static Vector2D Add(Vector2D value1, Vector2D value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static void Add(ref Vector2D value1, ref Vector2D value2, out Vector2D result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
        }

        public static Vector2D Barycentric(Vector2D value1, Vector2D value2, Vector2D value3, Fixed64 amount1, Fixed64 amount2)
        {
            return new Vector2D(Maths.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), Maths.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
        }

        public static void Barycentric(ref Vector2D value1, ref Vector2D value2, ref Vector2D value3, Fixed64 amount1, Fixed64 amount2, out Vector2D result)
        {
            result = new Vector2D(Maths.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), Maths.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
        }

        public static Vector2D CatmullRom(Vector2D value1, Vector2D value2, Vector2D value3, Vector2D value4, Fixed64 amount)
        {
            return new Vector2D(Maths.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), Maths.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
        }

        public static void CatmullRom(ref Vector2D value1, ref Vector2D value2, ref Vector2D value3, ref Vector2D value4, Fixed64 amount, out Vector2D result)
        {
            result = new Vector2D(Maths.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), Maths.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
        }

        public static Vector2D Clamp(Vector2D value1, Vector2D min, Vector2D max)
        {
            return new Vector2D(Maths.Clamp(value1.X, min.X, max.X), Maths.Clamp(value1.Y, min.Y, max.Y));
        }

        public static void Clamp(ref Vector2D value1, ref Vector2D min, ref Vector2D max, out Vector2D result)
        {
            result = new Vector2D(Maths.Clamp(value1.X, min.X, max.X), Maths.Clamp(value1.Y, min.Y, max.Y));
        }

        /// <summary>
        /// Returns FP precison distanve between two vectors
        /// </summary>
        public static Fixed64 Distance(Vector2D value1, Vector2D value2)
        {
            DistanceSquared(ref value1, ref value2, out var result);
            return result.Sqrt();
        }

        public static void Distance(ref Vector2D value1, ref Vector2D value2, out Fixed64 result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = result.Sqrt();
        }

        public static Fixed64 DistanceSquared(Vector2D value1, Vector2D value2)
        {
            DistanceSquared(ref value1, ref value2, out var result);
            return result;
        }

        public static void DistanceSquared(ref Vector2D value1, ref Vector2D value2, out Fixed64 result)
        {
            var delta = value1 - value2;
            result = delta.X * delta.X + delta.Y * delta.Y;
        }

        /// <summary>
        /// Devide first vector with the secund vector
        /// </summary>
        /// <param name="value1">
        /// A <see cref="Vector2D"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="Vector2D"/>
        /// </param>
        /// <returns>
        /// A <see cref="Vector2D"/>
        /// </returns>
        public static Vector2D Divide(Vector2D value1, Vector2D value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static void Divide(ref Vector2D value1, ref Vector2D value2, out Vector2D result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
        }

        public static Vector2D Divide(Vector2D value1, Fixed64 divider)
        {
            Fixed64 factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }

        public static void Divide(ref Vector2D value1, Fixed64 divider, out Vector2D result)
        {
            Fixed64 factor = 1 / divider;
            result.X = value1.X * factor;
            result.Y = value1.Y * factor;
        }

        #region 叉乘
        public static Fixed64 Cross(Vector2D value1, Vector2D value2)
        {
            return value1.X * value2.Y - value1.Y * value2.X;
        }
        public static void Cross(ref Vector2D value1, ref Vector2D value2, out Fixed64 result)
        {
            result = value1.X * value2.Y - value1.Y * value2.X;
        }
        #endregion

        #region 点乘
        public static Fixed64 Dot(Vector2D value1, Vector2D value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y;
        }

        public static void Dot(ref Vector2D value1, ref Vector2D value2, out Fixed64 result)
        {
            result = value1.X * value2.X + value1.Y * value2.Y;
        }
        #endregion

        public int CompareTo(Vector2D other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            return (obj is Vector2D) ? this == ((Vector2D)obj) : false;
        }

        public bool Equals(Vector2D other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)(X + Y);
        }

        public static Vector2D Hermite(Vector2D value1, Vector2D tangent1, Vector2D value2, Vector2D tangent2, Fixed64 amount)
        {
            Vector2D result = new Vector2D();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        public static void Hermite(ref Vector2D value1, ref Vector2D tangent1, ref Vector2D value2, ref Vector2D tangent2, Fixed64 amount, out Vector2D result)
        {
            result.X = Maths.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = Maths.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
        }

        public Fixed64 magnitude
        {
            get
            {
                DistanceSquared(ref this, ref zeroVector, out var result);
                return result.Sqrt();
            }
        }

        public static Vector2D ClampMagnitude(Vector2D vector, Fixed64 maxLength)
        {
            return Normalize(vector) * maxLength;
        }

        public Fixed64 LengthSquared()
        {
            DistanceSquared(ref this, ref zeroVector, out var result);
            return result;
        }

        public static Vector2D Lerp(Vector2D value1, Vector2D value2, Fixed64 amount)
        {
            amount = Maths.Clamp(amount, 0, 1);

            return new Vector2D(Maths.Lerp(value1.X, value2.X, amount), Maths.Lerp(value1.Y, value2.Y, amount));
        }

        public static Vector2D LerpUnclamped(Vector2D value1, Vector2D value2, Fixed64 amount)
        {
            return new Vector2D(Maths.Lerp(value1.X, value2.X, amount), Maths.Lerp(value1.Y, value2.Y, amount));
        }

        public static void LerpUnclamped(ref Vector2D value1, ref Vector2D value2, Fixed64 amount, out Vector2D result)
        {
            result = new Vector2D(Maths.Lerp(value1.X, value2.X, amount), Maths.Lerp(value1.Y, value2.Y, amount));
        }

        public static Vector2D Max(Vector2D value1, Vector2D value2)
        {
            return new Vector2D(Maths.Max(value1.X, value2.X), Maths.Max(value1.Y, value2.Y));
        }

        public static void Max(ref Vector2D value1, ref Vector2D value2, out Vector2D result)
        {
            result.X = Maths.Max(value1.X, value2.X);
            result.Y = Maths.Max(value1.Y, value2.Y);
        }

        public static Vector2D Min(Vector2D value1, Vector2D value2)
        {
            return new Vector2D(Maths.Min(value1.X, value2.X), Maths.Min(value1.Y, value2.Y));
        }

        public static void Min(ref Vector2D value1, ref Vector2D value2, out Vector2D result)
        {
            result.X = Maths.Min(value1.X, value2.X);
            result.Y = Maths.Min(value1.Y, value2.Y);
        }

        public void Scale(Vector2D other)
        {
            this.X = X * other.X;
            this.Y = Y * other.Y;
        }

        public static Vector2D Scale(Vector2D value1, Vector2D value2)
        {
            Vector2D result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;

            return result;
        }

        public static Vector2D Multiply(Vector2D value1, Vector2D value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static Vector2D Multiply(Vector2D value1, Fixed64 scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref Vector2D value1, Fixed64 scaleFactor, out Vector2D result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
        }

        public static void Multiply(ref Vector2D value1, ref Vector2D value2, out Vector2D result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
        }

        public static Vector2D Negate(Vector2D value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static void Negate(ref Vector2D value, out Vector2D result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
        }

        public void Normalize()
        {
            Normalize(ref this, out this);
        }

        public static Vector2D Normalize(Vector2D value)
        {
            Normalize(ref value, out value);
            return value;
        }

        public Vector2D normalized
        {
            get
            {
                Normalize(ref this, out var result);
                return result;
            }
        }

        public static void Normalize(ref Vector2D value, out Vector2D result)
        {
            DistanceSquared(ref value, ref zeroVector, out var factor);

            // 溢出  小于0  当做0计算
            factor = factor < 0 ? Fixed64.Zero : factor.Sqrt().Reciprocal();
            result.X = value.X * factor;
            result.Y = value.Y * factor;
        }

        public static Vector2D SmoothStep(Vector2D value1, Vector2D value2, Fixed64 amount)
        {
            return new Vector2D(Maths.SmoothStep(value1.X, value2.X, amount), Maths.SmoothStep(value1.Y, value2.Y, amount));
        }

        public static void SmoothStep(ref Vector2D value1, ref Vector2D value2, Fixed64 amount, out Vector2D result)
        {
            result = new Vector2D(Maths.SmoothStep(value1.X, value2.X, amount), Maths.SmoothStep(value1.Y, value2.Y, amount));
        }

        public static Vector2D Subtract(Vector2D value1, Vector2D value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static void Subtract(ref Vector2D value1, ref Vector2D value2, out Vector2D result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
        }

        public static Fixed64 Angle(Vector2D a, Vector2D b) => Maths.AcosDeg(a.normalized * b.normalized);

        public Vector3D ToTSVector()
        {
            return new Vector3D(this.X, this.Y, 0);
        }

        public override string ToString() => $"({X:f1}, {Y:f1})";
        #endregion Public Methods

        #region Operators
        public static Vector2D operator -(Vector2D value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public static bool operator ==(Vector2D value1, Vector2D value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }

        public static bool operator !=(Vector2D value1, Vector2D value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }

        public static Vector2D operator +(Vector2D value1, Vector2D value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static Vector2D operator -(Vector2D value1, Vector2D value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static Fixed64 operator *(Vector2D value1, Vector2D value2)
        {
            return Vector2D.Dot(value1, value2);
        }

        public static Vector2D operator *(Vector2D value, Fixed64 scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static Vector2D operator *(Fixed64 scaleFactor, Vector2D value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static Vector2D operator /(Vector2D value1, Vector2D value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static Vector2D operator /(Vector2D value1, Fixed64 divider)
        {
            Fixed64 factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }
        #endregion Operators
    }
}