using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 二维整数向量
    /// </summary>
    [Serializable]
    public struct Vector2DInt : IEquatable<Vector2DInt>, IComparable<Vector2DInt>, IFormattable
    {
        #region 字段/初始化
        public static readonly Vector2DInt Zero = new(0, 0);
        public static readonly Vector2DInt One = new(1, 1);
        public static readonly Vector2DInt Right = new(1, 0);
        public static readonly Vector2DInt Left = new(-1, 0);
        public static readonly Vector2DInt Up = new(0, 1);
        public static readonly Vector2DInt Down = new(0, -1);
        public static readonly Vector2DInt Infinitesimal = new(int.MinValue, int.MinValue);
        public static readonly Vector2DInt Infinity = new(int.MaxValue, int.MaxValue);

        public int X;
        public int Y;

        public Vector2DInt(int x)
        {
            X = x;
            Y = 0;
        }
        public Vector2DInt(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                _ => throw new IndexOutOfRangeException($"Invalid Vector2DInt index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Vector2DInt index:{index}!");
                }
            }
        }
        public void Set(int x, int y)
        {
            X = x;
            Y = y;
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 模长的平方
        /// </summary>
        public readonly int SqrMagnitude() => X * X + Y * Y;
        /// <summary>
        /// 模长
        /// </summary>
        public readonly Fixed64 Magnitude() => ((Fixed64)SqrMagnitude()).Sqrt();
        /// <summary>
        /// 返回两点之间的距离的平方
        /// </summary>
        public static int SqrDistance(Vector2DInt lhs, Vector2DInt rhs) => (lhs - rhs).SqrMagnitude();
        /// <summary>
        /// 返回两点之间的距离
        /// </summary>
        public static Fixed64 Distance(Vector2DInt lhs, Vector2DInt rhs) => ((Fixed64)(lhs - rhs).SqrMagnitude()).Sqrt();

        /// <summary>
        /// 点乘
        /// </summary>
        public static int Dot(Vector2DInt lhs, Vector2DInt rhs) => lhs.X * rhs.X + lhs.Y * rhs.Y;
        /// <summary>
        /// 叉乘
        /// </summary>
        public static int Cross(Vector2DInt lhs, Vector2DInt rhs) => lhs.X * rhs.Y - lhs.Y * rhs.X;
        /// <summary>
        /// 将两个向量的分量相乘
        /// </summary>
        public static Vector2DInt Scale(Vector2DInt lhs, Vector2DInt rhs) => new()
        {
            X = lhs.X * rhs.X,
            Y = lhs.Y * rhs.Y,
        };

        /// <summary>
        /// 符号值
        /// </summary>
        public readonly Vector2DInt Sign() => new(Math.Sign(X), Math.Sign(Y));
        /// <summary>
        /// 绝对值
        /// </summary>
        public readonly Vector2DInt Abs() => new(Math.Abs(X), Math.Abs(Y));
        /// <summary>
        /// 返回垂直于该向量的向量，对于正Y轴向上的坐标系来说，结果始终沿逆时针方向旋转90度
        /// </summary>
        public readonly Vector2DInt Perpendicular() => new(-Y, X);
        /// <summary>
        /// 从法线定义的向量反射一个向量
        /// </summary>
        public static Vector2DInt Reflect(Vector2DInt inDirection, Vector2DInt inNormal)
        {
            int dot = Dot(inDirection, inNormal) << 1;
            return new Vector2DInt(inDirection.X - dot * inNormal.X, inDirection.Y - dot * inNormal.Y);
        }

        /// <summary>
        /// 区间值更正
        /// </summary>
        public readonly Vector2DInt Clamp(Vector2DInt min, Vector2DInt max) => new()
        {
            X = Math.Clamp(X, min.X, max.X),
            Y = Math.Clamp(Y, min.Y, max.Y),
        };
        /// <summary>
        /// 较小值
        /// </summary>
        public static Vector2DInt Min(Vector2DInt lsh, Vector2DInt rsh) => new()
        {
            X = Math.Min(lsh.X, rsh.X),
            Y = Math.Min(lsh.Y, rsh.Y),
        };
        /// <summary>
        /// 较大值
        /// </summary>
        public static Vector2DInt Max(Vector2DInt lsh, Vector2DInt rsh) => new()
        {
            X = Math.Max(lsh.X, rsh.X),
            Y = Math.Max(lsh.Y, rsh.Y),
        };
        #endregion

        #region 隐式转换/显示转换/运算符重载
#if UNITY_5_3_OR_NEWER
        public static implicit operator UnityEngine.Vector2(Vector2DInt value) => new(value.X, value.Y);
        public static explicit operator Vector2DInt(UnityEngine.Vector2 value) => new((int)value.x, (int)value.y);

        public static implicit operator UnityEngine.Vector2Int(Vector2DInt value) => new(value.X, value.Y);
        public static implicit operator Vector2DInt(UnityEngine.Vector2Int value) => new(value.x, value.y);
#endif

        public static implicit operator System.Numerics.Vector2(Vector2DInt value) => new(value.X, value.Y);
        public static explicit operator Vector2DInt(System.Numerics.Vector2 value) => new((int)value.X, (int)value.Y);

        public static implicit operator Vector2D(Vector2DInt value) => new(value.X, value.Y);
        public static explicit operator Vector2DInt(in Vector2D value) => new((int)value.X, (int)value.Y);

        public static Vector2DInt operator +(Vector2DInt value) => value;
        public static Vector2DInt operator -(Vector2DInt value) => new(-value.X, -value.Y);
        public static Vector2DInt operator +(Vector2DInt lhs, Vector2DInt rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static Vector2DInt operator -(Vector2DInt lhs, Vector2DInt rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static Vector2DInt operator *(Vector2DInt lhs, int rhs) => new(lhs.X * rhs, lhs.Y * rhs);
        public static Vector2DInt operator *(int lhs, Vector2DInt rhs) => new(lhs * rhs.X, lhs * rhs.Y);
        public static Vector2DInt operator /(Vector2DInt lhs, int rhs) => new(lhs.X / rhs, lhs.Y / rhs);

        public static bool operator ==(Vector2DInt lhs, Vector2DInt rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(Vector2DInt lhs, Vector2DInt rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Vector2DInt other && this == other;
        public readonly override int GetHashCode() => X ^ Y;
        public readonly bool Equals(Vector2DInt other) => this == other;
        public readonly int CompareTo(Vector2DInt other)
        {
            int match0 = X.CompareTo(other.X);
            if (match0 != 0)
                return match0;

            int match1 = Y.CompareTo(other.Y);
            if (match1 != 0)
                return match1;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"({X.ToString(format, provider)}, {Y.ToString(format, provider)})";
        #endregion
    }
}