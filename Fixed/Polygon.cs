using Eevee.Collection;
using Eevee.Define;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Eevee.Fixed
{
    /// <summary>
    /// 确定性的多边形
    /// </summary>
    public readonly struct Polygon : IEquatable<Polygon>, IComparable<Polygon>, IFormattable
    {
        #region 字段/构造方法
        private readonly ReadOnlyArray<Vector2D> _points;

        public Polygon(in ReadOnlyArray<Vector2D> points)
        {
            Check.Polygon(points.Count);
            _points = points;
        }
        public ref Vector2D this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _points.RefGet(index);
        }
        #endregion

        #region 基础方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointCount() => _points.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SideCount() => _points.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Left()
        {
            var value = _points.RefGet(0).X;
            for (int i = 1; i < _points.Count; ++i)
            {
                ref var point = ref _points.RefGet(i);
                if (point.X < value)
                    value = point.X;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Right()
        {
            var value = _points.RefGet(0).X;
            for (int i = 1; i < _points.Count; ++i)
            {
                ref var point = ref _points.RefGet(i);
                if (point.X > value)
                    value = point.X;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Bottom()
        {
            var value = _points.RefGet(0).Y;
            for (int i = 1; i < _points.Count; ++i)
            {
                ref var point = ref _points.RefGet(i);
                if (point.Y < value)
                    value = point.Y;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed64 Top()
        {
            var value = _points.RefGet(0).Y;
            for (int i = 1; i < _points.Count; ++i)
            {
                ref var point = ref _points.RefGet(i);
                if (point.Y > value)
                    value = point.Y;
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Center()
        {
            CountPeek(out var xMin, out var xMax, out var yMin, out var yMax);
            return new Vector2D(xMax + xMin >> 1, yMax + yMin >> 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D Size() // 尺寸
        {
            CountPeek(out var xMin, out var xMax, out var yMin, out var yMax);
            return new Vector2D(xMax - xMin, yMax - yMin);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D HalfSize() // 一半尺寸
        {
            CountPeek(out var xMin, out var xMax, out var yMin, out var yMax);
            return new Vector2D(xMax - xMin >> 1, yMax - yMin >> 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<Vector2D> GetPoints() => _points.AsSpan();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CountPeek(out Fixed64 xMin, out Fixed64 xMax, out Fixed64 yMin, out Fixed64 yMax)
        {
            var point0 = _points.RefGet(0);
            xMin = point0.X;
            xMax = point0.X;
            yMin = point0.Y;
            yMax = point0.Y;

            for (int i = 1; i < _points.Count; ++i)
            {
                ref var point = ref _points.RefGet(i);

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
        public static bool operator ==(in Polygon lhs, in Polygon rhs)
        {
            if (lhs._points.Count != rhs._points.Count)
                return false;
            for (int i = 0; i < lhs._points.Count; ++i)
                if (lhs._points.RefGet(i) != rhs._points.RefGet(i))
                    return false;
            return true;
        }
        public static bool operator !=(in Polygon lhs, in Polygon rhs)
        {
            if (lhs._points.Count != rhs._points.Count)
                return true;
            for (int i = 0; i < lhs._points.Count; ++i)
                if (lhs._points.RefGet(i) != rhs._points.RefGet(i))
                    return true;
            return false;
        }
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is Polygon other && this == other;

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var point in _points)
                hashCode ^= point.GetHashCode();
            return hashCode;
        }
        public bool Equals(Polygon other) => this == other;
        public int CompareTo(Polygon other)
        {
            // todo eevee
            throw new NotImplementedException();
        }

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