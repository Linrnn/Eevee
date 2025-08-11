using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    #region Point
    internal readonly struct QuadTreePoint2CircleChecker : IQuadTreeChecker
    {
        internal readonly Vector2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreePoint2CircleChecker(Vector2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(Converts.AsCircleInt(in boundary), Shape);
    }

    internal readonly struct QuadTreePoint2AABBChecker : IQuadTreeChecker
    {
        internal readonly Vector2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreePoint2AABBChecker(Vector2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
    }
    #endregion

    #region Circle
    internal readonly struct QuadTreeCircle2CircleChecker : IQuadTreeChecker
    {
        internal readonly CircleInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreeCircle2CircleChecker(in CircleInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(Converts.AsCircleInt(in boundary), in Shape);
    }

    internal readonly struct QuadTreeCircle2AABBChecker : IQuadTreeChecker
    {
        internal readonly CircleInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreeCircle2AABBChecker(in CircleInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
    }
    #endregion

    #region AABB
    internal readonly struct QuadTreeAABB2CircleChecker : IQuadTreeChecker
    {
        internal readonly AABB2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreeAABB2CircleChecker(in AABB2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, Converts.AsCircleInt(in boundary));
    }

    internal readonly struct QuadTreeAABB2AABBChecker : IQuadTreeChecker
    {
        internal readonly AABB2DInt Shape;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreeAABB2AABBChecker(in AABB2DInt shape) => Shape = shape;
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
    }
    #endregion

    #region OBB
    internal readonly struct QuadTreeOBB2CircleChecker : IQuadTreeChecker
    {
        internal readonly OBB2DInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreeOBB2CircleChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;

            Origin = shape;
            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary)
        {
            var shape = Converts.AsCircleInt(in boundary);
            return Geometry.Intersect(in Shape, in shape) && Geometry.Intersect(in Origin, in shape);
        }
    }

    internal readonly struct QuadTreeOBB2AABBChecker : IQuadTreeChecker
    {
        internal readonly OBB2DInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreeOBB2AABBChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;

            Origin = shape;
            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary) && Checker.Intersect(in boundary);
    }
    #endregion

    #region Polygon
    internal readonly struct QuadTreePolygon2CircleChecker : IQuadTreeChecker
    {
        internal readonly PolygonInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreePolygon2CircleChecker(in PolygonInt shape)
        {
            var checker = new PolygonIntIntersectChecker(in shape, true, true);

            Origin = shape;
            Shape = AABB2DInt.Create(checker.Shape.Left(), checker.Shape.Right(), checker.Shape.Bottom(), checker.Shape.Top());
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary)
        {
            var shape = Converts.AsCircleInt(in boundary);
            return Geometry.Intersect(in Shape, in shape) && Checker.Intersect(in shape);
        }
    }

    internal readonly struct QuadTreePolygon2AABBChecker : IQuadTreeChecker
    {
        internal readonly PolygonInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal QuadTreePolygon2AABBChecker(in PolygonInt shape)
        {
            var checker = new PolygonIntIntersectChecker(in shape, true, false);

            Origin = shape;
            Shape = AABB2DInt.Create(checker.Shape.Left(), checker.Shape.Right(), checker.Shape.Bottom(), checker.Shape.Top());
            Checker = checker;
        }
        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary) && Checker.Intersect(in boundary);
    }
    #endregion
}