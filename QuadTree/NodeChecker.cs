using Eevee.Fixed;
using System.Runtime.CompilerServices;

namespace Eevee.QuadTree
{
    #region Point
    internal readonly struct Point2CircleNodeChecker : INodeChecker
    {
        internal readonly Vector2DInt Shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Point2CircleNodeChecker(Vector2DInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(Converts.AsCircleInt(in boundary), Shape);
    }

    internal readonly struct Point2AABBNodeChecker : INodeChecker
    {
        internal readonly Vector2DInt Shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Point2AABBNodeChecker(Vector2DInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Contain(in boundary, Shape);
    }
    #endregion

    #region Circle
    internal readonly struct Circle2CircleNodeChecker : INodeChecker
    {
        internal readonly CircleInt Shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Circle2CircleNodeChecker(in CircleInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in Shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(Converts.AsCircleInt(in boundary), in Shape);
    }

    internal readonly struct Circle2AABBNodeChecker : INodeChecker
    {
        private readonly CircleInt _shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Circle2AABBNodeChecker(in CircleInt shape) => _shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in _shape);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in boundary, in _shape);
    }
    #endregion

    #region AABB
    internal readonly struct AABB2CircleNodeChecker : INodeChecker
    {
        internal readonly AABB2DInt Shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AABB2CircleNodeChecker(in AABB2DInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, Converts.AsCircleInt(in boundary));
    }

    internal readonly struct AABB2AABBNodeChecker : INodeChecker
    {
        internal readonly AABB2DInt Shape;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AABB2AABBNodeChecker(in AABB2DInt shape) => Shape = shape;

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
    }
    #endregion

    #region OBB
    internal readonly struct OBB2CircleNodeChecker : INodeChecker
    {
        internal readonly OBB2DInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OBB2CircleNodeChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;
            Origin = shape;
            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Origin, Converts.AsCircleInt(in boundary));
    }

    internal readonly struct OBB2AABBNodeChecker : INodeChecker
    {
        internal readonly OBB2DInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly OBBIntIntersectChecker Checker;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OBB2AABBNodeChecker(in OBB2DInt shape)
        {
            var checker = new OBBIntIntersectChecker(in shape);
            int w = checker.XMax() - checker.XMin() >> 1;
            int h = checker.YMax() - checker.YMin() >> 1;
            Origin = shape;
            Shape = new AABB2DInt(shape.X, shape.Y, w, h);
            Checker = checker;
        }

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary) && Geometry.Intersect(in Origin, in boundary);
    }
    #endregion

    #region Polygon
    internal readonly struct Polygon2CircleNodeChecker : INodeChecker
    {
        internal readonly PolygonInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Polygon2CircleNodeChecker(in PolygonInt shape)
        {
            var checker = new PolygonIntIntersectChecker(in shape);
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

    internal readonly struct Polygon2AABBNodeChecker : INodeChecker
    {
        internal readonly PolygonInt Origin;
        internal readonly AABB2DInt Shape;
        internal readonly PolygonIntIntersectChecker Checker;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Polygon2AABBNodeChecker(in PolygonInt shape)
        {
            var checker = new PolygonIntIntersectChecker(in shape);
            Origin = shape;
            Shape = AABB2DInt.Create(checker.Shape.Left(), checker.Shape.Right(), checker.Shape.Bottom(), checker.Shape.Top());
            Checker = checker;
        }

        public bool CheckNode(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary);
        public bool CheckElement(in AABB2DInt boundary) => Geometry.Intersect(in Shape, in boundary) && Checker.Intersect(in boundary);
    }
    #endregion
}