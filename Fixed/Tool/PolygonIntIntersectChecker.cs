using Eevee.Collection;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct PolygonIntIntersectChecker : IDisposable
    {
        internal readonly PolygonInt Shape;
        private readonly Vector2DInt[] _deltas;
        private readonly Segment2DInt[] _sides;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonIntIntersectChecker(in PolygonInt shape, bool countDeltas, bool countSides)
        {
            int count = shape.PointCount();
            Shape = shape;

            if (countDeltas && countSides)
            {
                var deltas = ArrayExt.SharedRent<Vector2DInt>(count);
                var sides = ArrayExt.SharedRent<Segment2DInt>(count);
                for (int i = 0, j = count - 1; i < count; j = i++)
                {
                    var pi = shape[i];
                    var pj = shape[j];
                    deltas[i] = pi - pj;
                    sides[i] = new Segment2DInt(pi, pj);
                }
                _deltas = deltas;
                _sides = sides;
            }
            else if (countDeltas)
            {
                var deltas = ArrayExt.SharedRent<Vector2DInt>(count);
                for (int i = 0, j = count - 1; i < count; j = i++)
                    deltas[i] = shape[i] - shape[j];
                _deltas = deltas;
                _sides = null;
            }
            else if (countSides)
            {
                var sides = ArrayExt.SharedRent<Segment2DInt>(count);
                for (int i = 0, j = count - 1; i < count; j = i++)
                    sides[i] = new Segment2DInt(shape[i], shape[j]);
                _deltas = null;
                _sides = sides;
            }
            else
            {
                _deltas = null;
                _sides = null;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _deltas?.SharedReturn();
            _sides?.SharedReturn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Contain(Vector2DInt shape) // 向量方向相同
        {
            int flag = 0;
            for (int count = Shape.PointCount(), i = 0; i < count; ++i)
            {
                int cross = Vector2DInt.Cross(shape - Shape[i], _deltas[i]);
                if (cross == 0)
                    continue;
                if (flag == 0)
                    flag = Math.Sign(cross);
                else if (flag != Math.Sign(cross))
                    return false;
            }

            return flag != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in CircleInt shape)
        {
            var center = shape.Center();
            var rSqr = (Fixed64)(shape.R * shape.R);
            foreach (var segment in _sides.AsReadOnlySpan())
                if (Geometry.SqrDistance(in segment, center) <= rSqr)
                    return true;
            return Contain(center);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt shape)
        {
            if (Contain(shape.LeftBottom()))
                return true;
            if (Contain(shape.RightBottom()))
                return true;
            if (Contain(shape.RightTop()))
                return true;
            if (Contain(shape.LeftTop()))
                return true;

            foreach (var point in Shape.GetPoints())
                if (Geometry.Contain(in shape, point))
                    return true;

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in OBB2DInt shape)
        {
            shape.RotatedCorner(out var p0, out var p1, out var p2, out var p3);

            if (Contain((Vector2DInt)p0))
                return true;
            if (Contain((Vector2DInt)p1))
                return true;
            if (Contain((Vector2DInt)p2))
                return true;
            if (Contain((Vector2DInt)p3))
                return true;

            foreach (var point in Shape.GetPoints())
                if (Geometry.Contain(in shape, point))
                    return true;

            return false;
        }
    }
}