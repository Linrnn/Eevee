using Eevee.Collection;
using Eevee.Define;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Eevee.Fixed
{
    /// <summary>
    /// 多边形
    /// </summary>
    public readonly struct PolygonInt : IEquatable<PolygonInt>, IComparable<PolygonInt>, IFormattable
    {
        #region 字段/构造方法
        private readonly ReadOnlyArray<Vector2DInt> _points;

        public PolygonInt(in ReadOnlyArray<Vector2DInt> points)
        {
            Check.Polygon(points.Count);
            _points = points;
        }
        public Vector2DInt this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _points.Get(index);
        }
        #endregion

        #region 基础方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointCount() => _points.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SideCount() => _points.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left()
        {
            int value = _points.Get(0).X;
            for (int i = 1; i < _points.Count; ++i)
            {
                var point = _points.Get(i);
                if (point.X < value)
                    value = point.X;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Right()
        {
            int value = _points.Get(0).X;
            for (int i = 1; i < _points.Count; ++i)
            {
                var point = _points.Get(i);
                if (point.X > value)
                    value = point.X;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Bottom()
        {
            int value = _points.Get(0).Y;
            for (int i = 1; i < _points.Count; ++i)
            {
                var point = _points.Get(i);
                if (point.Y < value)
                    value = point.Y;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Top()
        {
            int value = _points.Get(0).Y;
            for (int i = 1; i < _points.Count; ++i)
            {
                var point = _points.Get(i);
                if (point.Y > value)
                    value = point.Y;
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Center()
        {
            CountPeek(out int xMin, out int xMax, out int yMin, out int yMax);
            return new Vector2DInt(xMax + xMin >> 1, yMax + yMin >> 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt Size() // 尺寸
        {
            CountPeek(out int xMin, out int xMax, out int yMin, out int yMax);
            return new Vector2DInt(xMax - xMin, yMax - yMin);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2DInt HalfSize() // 一半尺寸
        {
            CountPeek(out int xMin, out int xMax, out int yMin, out int yMax);
            return new Vector2DInt(xMax - xMin >> 1, yMax - yMin >> 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<Vector2DInt> GetPoints() => _points.AsSpan();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CountPeek(out int xMin, out int xMax, out int yMin, out int yMax)
        {
            var point0 = _points.Get(0);
            xMin = point0.X;
            xMax = point0.X;
            yMin = point0.Y;
            yMax = point0.Y;

            for (int i = 1; i < _points.Count; ++i)
            {
                var point = _points.Get(i);

                if (point.X < xMin)
                    xMin = point.X;
                else if (point.X > xMax)
                    xMax = point.X;

                if (point.Y < yMin)
                    yMin = point.Y;
                else if (point.Y > yMax)
                    yMax = point.Y;
            }
        }
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static bool operator ==(in PolygonInt lhs, in PolygonInt rhs)
        {
            if (lhs._points.Count != rhs._points.Count)
                return false;
            for (int i = 0; i < lhs._points.Count; ++i)
                if (lhs._points.Get(i) != rhs._points.Get(i))
                    return false;
            return true;
        }
        public static bool operator !=(in PolygonInt lhs, in PolygonInt rhs)
        {
            if (lhs._points.Count != rhs._points.Count)
                return true;
            for (int i = 0; i < lhs._points.Count; ++i)
                if (lhs._points.Get(i) != rhs._points.Get(i))
                    return true;
            return false;
        }
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is PolygonInt other && this == other;

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var point in _points)
                hashCode ^= point.GetHashCode();
            return hashCode;
        }
        public bool Equals(PolygonInt other) => this == other;
        public int CompareTo(PolygonInt other) => PointCount().CompareTo(other.PointCount());

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider)
        {
            var sb = new StringBuilder(_points.Count * 10);
            sb.Append('[');
            foreach (var point in _points)
                sb.Append(point.ToString(format, provider));
            sb.Append(']');
            return sb.ToString();
        }
        #endregion
    }
}