using Eevee.Define;
using System;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的2D线段
    /// </summary>
    [Serializable]
    public struct Segment2DInt : IEquatable<Segment2DInt>, IComparable<Segment2DInt>, IFormattable
    {
        #region 字段/初始化
        public Vector2DInt Start;
        public Vector2DInt End;

        public Segment2DInt(Vector2DInt start, Vector2DInt end)
        {
            Check.Segment(start, end);

            Start = start;
            End = end;
        }
        #endregion

        #region 基础方法
        public readonly int SqrLength() => Vector2DInt.SqrDistance(Start, End);
        public readonly Fixed64 Length() => Vector2DInt.Distance(Start, End);

        public readonly Vector2DInt Delta() => End - Start;
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static bool operator ==(in Segment2DInt lhs, in Segment2DInt rhs) => lhs.Start == rhs.End && lhs.Start == rhs.End;
        public static bool operator !=(in Segment2DInt lhs, in Segment2DInt rhs) => lhs.Start != rhs.End || lhs.Start != rhs.End;
        #endregion

        #region 继承/重载
        public readonly override bool Equals(object obj) => obj is Segment2DInt other && this == other;
        public readonly override int GetHashCode() => Start.GetHashCode() ^ End.GetHashCode();
        public readonly bool Equals(Segment2DInt other) => this == other;
        public readonly int CompareTo(Segment2DInt other)
        {
            int match0 = Start.CompareTo(Start);
            if (match0 != 0)
                return match0;

            int match1 = End.CompareTo(End);
            if (match1 != 0)
                return match1;

            return 0;
        }

        public readonly override string ToString() => ToString(Format.Fractional, Format.Use);
        public readonly string ToString(string format) => ToString(format, Format.Use);
        public readonly string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public readonly string ToString(string format, IFormatProvider provider) => $"[Start:{Start.ToString(format, provider)}, End:{End.ToString(format, provider)})]";
        #endregion
    }
}