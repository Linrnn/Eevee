using Eevee.Collection;
using System;
using System.Runtime.CompilerServices;

namespace Eevee.Fixed
{
    internal readonly struct PolygonIntIntersectChecker : IDisposable
    {
        internal readonly PolygonInt Shape;
        private readonly Vector2DInt[] _sides;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonIntIntersectChecker(in PolygonInt shape)
        {
            int count = shape.SideCount();
            var sides = ArrayExt.SharedRent<Vector2DInt>(count);
            for (int i = 0, j = count - 1; i < count; j = i++)
                sides[i] = shape[i] - shape[j];
            Shape = shape;
            _sides = sides;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _sides.SharedReturn();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Contain(Vector2DInt shape) // 向量方向相同
        {
            for (int flag = 0, count = Shape.PointCount(), i = 0; i < count; ++i)
            {
                int cross = Vector2DInt.Cross(shape - Shape[i], _sides[i]);
                if (cross == 0)
                    continue;
                if (flag == 0)
                    flag = cross;
                else if (flag != cross)
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

            foreach (var point in Shape.Points)
                if (Geometry.Contain(in shape, point))
                    return true;

            return false;
        }
    }
}