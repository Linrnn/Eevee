using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的二维向量
    /// </summary>
    [Serializable]
    public struct Vector2D : IEquatable<Vector2D>, IComparable<Vector2D>
    {
        #region 字段/初始化
        public static readonly Vector2D Zero = new(0, 0);
        public static readonly Vector2D One = new(1, 1);
        public static readonly Vector2D Right = new(1, 0);
        public static readonly Vector2D Left = new(-1, 0);
        public static readonly Vector2D Up = new(0, 1);
        public static readonly Vector2D Down = new(0, -1);

        public Fixed64 X;
        public Fixed64 Y;

        public Fixed64 this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                _ => throw new IndexOutOfRangeException($"Invalid Vector2D index:{index}!"),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    default: throw new IndexOutOfRangeException($"Invalid Vector2D index:{index}!");
                }
            }
        }

        public Vector2D(Fixed64 value)
        {
            X = value;
            Y = value;
        }
        public Vector2D(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
        }
        public void Set(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
        }
        #endregion

        #region 基础方法
        /// <summary>
        /// 模长
        /// </summary>
        public readonly Fixed64 Magnitude() => X * X + Y * Y;
        /// <summary>
        /// 模长的平方
        /// </summary>
        public readonly Fixed64 SqrMagnitude() => Magnitude().Sqrt();
        /// <summary>
        /// 返回两点之间的距离
        /// </summary>
        public static Fixed64 Distance(in Vector2D lhs, in Vector2D rhs) => (lhs - rhs).Magnitude();
        /// <summary>
        /// 返回两点之间的距离的平方
        /// </summary>
        public static Fixed64 SqrDistance(in Vector2D lhs, in Vector2D rhs) => (lhs - rhs).Magnitude().Sqrt();

        /// <summary>
        /// 返回副本，其大小被限制为输入值
        /// </summary>
        public readonly Vector2D ClampMagnitude(Fixed64 maxLength)
        {
            switch (maxLength.RawValue)
            {
                case < 0L: throw new ArgumentOutOfRangeException(nameof(maxLength), $"maxLength:{maxLength} < 0");
                case 0L: return Zero;
            }

            var sqrMagnitude = SqrMagnitude();
            if (sqrMagnitude.RawValue == 0L)
                return Zero;

            var sqrMaxLength = maxLength.Sqr();
            if (sqrMaxLength >= sqrMagnitude)
                return this;

            return this * (sqrMaxLength / sqrMagnitude).Sqrt();
        }
        /// <summary>
        /// 返回该向量的模长为1的向量
        /// </summary>
        public readonly Vector2D Normalized() => this * Magnitude().Reciprocal();
        /// <summary>
        /// 使该向量的模长为1
        /// </summary>
        public void Normalize() => this = Normalized();

        /// <summary>
        /// 返回垂直于该向量的向量，对于正Y轴向上的坐标系来说，结果始终沿逆时针方向旋转90度
        /// </summary>
        public readonly Vector2D Perpendicular() => new(-Y, X);

        /// <summary>
        /// 点乘
        /// </summary>
        public static Fixed64 Dot(in Vector2D lhs, in Vector2D rhs) => lhs * rhs;
        /// <summary>
        /// 叉乘
        /// </summary>
        public static Fixed64 Cross(in Vector2D lhs, in Vector2D rhs) => lhs.X * rhs.Y - lhs.Y * rhs.X;
        /// <summary>
        /// 将两个向量的分量相乘
        /// </summary>
        public static Vector2D Scale(in Vector2D lhs, in Vector2D rhs) => new(lhs.X * rhs.X, lhs.Y * rhs.Y);
        /// <summary>
        /// 从法线定义的向量反射一个向量
        /// </summary>
        public static Vector2D Reflect(in Vector2D direction, in Vector2D normal)
        {
            var dot = direction * normal << 1;
            return new Vector2D(direction.X - dot * normal.X, direction.Y - dot * normal.Y);
        }

        public readonly bool IsZero() => SqrMagnitude().RawValue == 0L;
        public readonly bool IsNearlyZero() => SqrMagnitude().RawValue <= Const.Epsilon;
        #endregion Public Methods

        #region 运算符重载
        public static Vector2D operator +(in Vector2D value) => value;
        public static Vector2D operator -(in Vector2D value) => new(-value.X, -value.Y);
        public static Vector2D operator +(in Vector2D lhs, in Vector2D rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static Vector2D operator -(in Vector2D lhs, in Vector2D rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static Fixed64 operator *(in Vector2D lhs, in Vector2D rhs) => lhs.X * rhs.X + lhs.Y * rhs.Y;
        public static Vector2D operator *(in Vector2D lhs, in Fixed64 rhs) => new(lhs.X * rhs, lhs.Y * rhs);
        public static Vector2D operator *(in Vector2D lhs, long rhs) => new(lhs.X * rhs, lhs.Y * rhs);
        public static Vector2D operator *(in Fixed64 lhs, in Vector2D rhs) => new(lhs * rhs.X, lhs * rhs.Y);
        public static Vector2D operator *(long lhs, in Vector2D rhs) => new(lhs * rhs.X, lhs * rhs.Y);
        public static Vector2D operator /(in Vector2D lhs, Fixed64 rhs) => new(lhs.X / rhs, lhs.Y / rhs);
        public static Vector2D operator /(in Vector2D lhs, long rhs) => new(lhs.X / rhs, lhs.Y / rhs);

        public static bool operator ==(in Vector2D lhs, in Vector2D rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(in Vector2D lhs, in Vector2D rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;
        #endregion Operators

        #region 继承重载
        public readonly override bool Equals(object obj) => obj is Vector2D other && this == other;
        public readonly override int GetHashCode() => HashCode.Combine(X.RawValue, Y.RawValue);
        public readonly override string ToString() => $"({X}, {Y})";

        public readonly bool Equals(Vector2D other) => this == other;
        public readonly int CompareTo(Vector2D other)
        {
            int match1 = X.CompareTo(other.X);
            if (match1 != 0)
                return match1;

            int match2 = Y.CompareTo(other.Y);
            if (match2 != 0)
                return match2;

            return 0;
        }
        #endregion
    }
}