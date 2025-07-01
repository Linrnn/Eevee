using Eevee.Collection;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct PolygonIntersectChecker : IDisposable
    {
        internal readonly Polygon Shape;
        private readonly Vector2D[] _deltas;
        private readonly Segment2D[] _sides;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonIntersectChecker(in Polygon shape, bool countDeltas, bool countSides)
        {
            int count = shape.PointCount();
            Shape = shape;

            if (countDeltas && countSides)
            {
                var deltas = ArrayExt.SharedRent<Vector2D>(count);
                var sides = ArrayExt.SharedRent<Segment2D>(count);
                for (int i = 0, j = count - 1; i < count; j = i++)
                {
                    ref var pi = ref shape[i];
                    ref var pj = ref shape[j];
                    deltas[i] = pi - shape[j];
                    sides[i] = new Segment2D(in pi, in pj);
                }
                _deltas = deltas;
                _sides = sides;
            }
            else if (countDeltas)
            {
                var deltas = ArrayExt.SharedRent<Vector2D>(count);
                for (int i = 0, j = count - 1; i < count; j = i++)
                    deltas[i] = shape[i] - shape[j];
                _deltas = deltas;
                _sides = null;
            }
            else if (countSides)
            {
                var sides = ArrayExt.SharedRent<Segment2D>(count);
                for (int i = 0, j = count - 1; i < count; j = i++)
                    sides[i] = new Segment2D(in shape[i], in shape[j]);
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
        internal bool Contain(in Vector2D shape) // 向量方向相同
        {
            int flag = 0;
            for (int count = Shape.PointCount(), i = 0; i < count; ++i)
            {
                var cross = Vector2D.Cross(shape - Shape[i], in _deltas[i]);
                if (cross.RawValue == 0)
                    continue;
                if (flag == 0)
                    flag = cross.Sign();
                else if (flag != cross.Sign())
                    return false;
            }

            return flag != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in Circle shape)
        {
            var center = shape.Center();
            var rSqr = shape.R.Sqr();
            foreach (var segment in _sides.AsReadOnlySpan())
                if (Geometry.SqrDistance(in segment, in center) <= rSqr)
                    return true;
            return Contain(in center);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2D shape)
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
                if (Geometry.Contain(in shape, in point))
                    return true;

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in OBB2D shape)
        {
            shape.RotatedCorner(out var p0, out var p1, out var p2, out var p3);

            if (Contain(in p0))
                return true;
            if (Contain(in p1))
                return true;
            if (Contain(in p2))
                return true;
            if (Contain(in p3))
                return true;

            foreach (var point in Shape.GetPoints())
                if (Geometry.Contain(in shape, in point))
                    return true;

            return false;
        }
    }
}