using Eevee.Collection;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct PolygonIntersectChecker : IDisposable
    {
        internal readonly Polygon Shape;
        private readonly Vector2D[] _sides;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonIntersectChecker(in Polygon shape)
        {
            int count = shape.SideCount();
            var sides = ArrayExt.SharedRent<Vector2D>(count);
            for (int i = 0, j = count - 1; i < count; j = i++)
                sides[i] = shape[i] - shape[j];
            Shape = shape;
            _sides = sides;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _sides.SharedReturn();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Contain(in Vector2D shape) // 向量方向相同
        {
            for (int flag = 0, count = Shape.PointCount(), i = 0; i < count; ++i)
            {
                var cross = Vector2D.Cross(shape - Shape[i], in _sides[i]);
                if (cross.RawValue == 0)
                    continue;
                if (flag == 0)
                    flag = cross.Sign();
                else if (flag != cross.Sign())
                    return false;
            }

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in CircleInt shape)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
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

            foreach (var point in Shape.Points)
                if (Geometry.Contain(in shape, in point))
                    return true;

            return false;
        }
    }
}