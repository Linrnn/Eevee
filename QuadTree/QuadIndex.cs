using Eevee.Define;
using System;

namespace Eevee.QuadTree
{
    /// <summary>
    /// 四叉树坐标
    /// </summary>
    public readonly struct QuadIndex : IEquatable<QuadIndex>, IComparable<QuadIndex>, IFormattable
    {
        #region 字段/初始化
        public static readonly QuadIndex Invalid = new(-1, -1, -1); // 无效索引
        public static readonly QuadIndex Root = new(0, 0, 0); // 根节点索引

        public readonly int Depth; // 所在层深度
        public readonly int X; // 所在层的X坐标
        public readonly int Y; // 所在层的Y坐标

        public QuadIndex(int depth, int x, int y)
        {
            Depth = depth;
            X = x;
            Y = y;
        }
        #endregion

        #region 基础方法
        public QuadIndex Parent() => this == Root ? Invalid : new QuadIndex(Depth - 1, X >> 1, Y >> 1);

        public bool IsValid() => Depth >= 0 && X >= 0 && Y >= 0; // 有效索引
        public static bool IsValid(int depth, int x, int y) => depth >= 0 && x >= 0 && y >= 0; // 有效索引
        #endregion

        #region 运算符重载
        public static bool operator ==(in QuadIndex lhs, in QuadIndex rhs) => lhs.Depth == rhs.Depth && lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(in QuadIndex lhs, in QuadIndex rhs) => lhs.Depth != rhs.Depth || lhs.X != rhs.X || lhs.Y != rhs.Y;
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is QuadIndex other && this == other;
        public override int GetHashCode() => Depth ^ X ^ Y;
        public bool Equals(QuadIndex other) => this == other;
        public int CompareTo(QuadIndex other)
        {
            int match0 = Depth.CompareTo(other.Depth);
            if (match0 != 0)
                return match0;

            int match1 = X.CompareTo(other.X);
            if (match1 != 0)
                return match1;

            int match2 = Y.CompareTo(other.Y);
            if (match2 != 0)
                return match2;

            return 0;
        }

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider) => $"Depth:{Depth.ToString(format, provider)}, X:{X.ToString(format, provider)}, Y:{Y.ToString(format, provider)}";
        #endregion
    }
}