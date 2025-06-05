using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Eevee.Fixed
{
    internal readonly unsafe struct PolygonIntIntersectChecker : IDisposable
    {
        internal readonly PolygonInt Shape;
        private readonly Vector2DInt* _sides;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PolygonIntIntersectChecker(in PolygonInt shape)
        {
            var sides = (Vector2DInt*)Marshal.AllocHGlobal(sizeof(Vector2DInt) * shape.Points.Count);
            int last = shape.Points.Count - 1;
            for (int i = 0; i < last; ++i)
                sides[i] = shape[i] - shape[i + 1];
            sides[last] = shape[last] - shape[0];

            Shape = shape;
            _sides = sides;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Marshal.FreeHGlobal((IntPtr)_sides);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in CircleInt shape)
        {
            // todo eevee 未实现
            throw new NotImplementedException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersect(in AABB2DInt shape)
        {
            if (SameDirection(shape.LeftBottom()))
                return true;
            if (SameDirection(shape.RightBottom()))
                return true;
            if (SameDirection(shape.RightTop()))
                return true;
            if (SameDirection(shape.LeftTop()))
                return true;

            foreach (var point in Shape.Points)
                if (Geometry.Contain(in shape, point))
                    return true;

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SameDirection(Vector2DInt point) // 向量方向相同
        {
            for (int flag = 0, count = Shape.PointCount(), i = 0; i < count; ++i)
            {
                int cross = Vector2DInt.Cross(point - Shape[i], _sides[i]);
                if (cross == 0)
                    continue;

                if (flag == 0)
                    flag = cross;
                else if (flag != cross)
                    return false;
            }

            return true;
        }
    }
}