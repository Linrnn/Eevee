using Eevee.Collection;
using Eevee.Define;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    /// <summary>
    /// 多边形
    /// </summary>
    public readonly struct PolygonInt : IEquatable<PolygonInt>, IComparable<PolygonInt>, IFormattable
    {
        #region 字段/构造方法
        public readonly ReadOnlyArray<Vector2DInt> Points;

        public PolygonInt(in ReadOnlyArray<Vector2DInt> points)
        {
            Check.Polygon(points.Count);
            Points = points;
        }
        public Vector2DInt this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Points.Get(index);
        }
        #endregion

        #region 基础方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointCount() => Points.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SideCount() => Points.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left()
        {
            int value = Points.Get(0).X;
            for (int i = 1; i < Points.Count; ++i)
            {
                var point = Points.Get(i);
                if (point.X < value)
                    value = point.X;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Right()
        {
            int value = Points.Get(0).X;
            for (int i = 1; i < Points.Count; ++i)
            {
                var point = Points.Get(i);
                if (point.X > value)
                    value = point.X;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Bottom()
        {
            int value = Points.Get(0).Y;
            for (int i = 1; i < Points.Count; ++i)
            {
                var point = Points.Get(i);
                if (point.Y < value)
                    value = point.Y;
            }
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Top()
        {
            int value = Points.Get(0).Y;
            for (int i = 1; i < Points.Count; ++i)
            {
                var point = Points.Get(i);
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
        private void CountPeek(out int xMin, out int xMax, out int yMin, out int yMax)
        {
            var point0 = Points.Get(0);
            xMin = point0.X;
            xMax = point0.X;
            yMin = point0.Y;
            yMax = point0.Y;

            for (int i = 1; i < Points.Count; ++i)
            {
                var point = Points.Get(i);

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
            // todo eevee
            throw new NotImplementedException();
        }
        public static bool operator !=(in PolygonInt lhs, in PolygonInt rhs)
        {
            // todo eevee
            throw new NotImplementedException();
        }
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => obj is PolygonInt other && this == other;

        public override int GetHashCode()
        {
            // todo eevee
            throw new NotImplementedException();
        }
        public bool Equals(PolygonInt other) => this == other;
        public int CompareTo(PolygonInt other)
        {
            // todo eevee
            throw new NotImplementedException();
        }

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider)
        {
            // todo eevee
            throw new NotImplementedException();
        }
        #endregion
    }
}