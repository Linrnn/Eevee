#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using System;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal static class QuadTreeDraw
    {
        internal static float Height;

        private static IQuadTreeDrawProxy Proxy => QuadTreeGetter.Proxy;
        private static DrawData DrawData => new(1F / Proxy.Manager.Scale, Height);

        internal static void Label(Vector2 point, string label, in Color color) => ShapeDraw.Label(point, DrawData, label, in color);
        internal static void Point(Vector2 shape, in Color color) => ShapeDraw.Point(shape, DrawData, in color);
        internal static void Circle(Vector2 point, float radius, in Color color) => ShapeDraw.Circle(point, radius, DrawData, in color);
        internal static void AABB(Vector2 point, Vector2 extents, in Color color) => ShapeDraw.AABB(point, extents, DrawData, in color);
        internal static void OBB(Vector2 point, Vector2 extents, float angle, in Color color) => ShapeDraw.OBB(point, extents, angle, DrawData, in color);

        internal static void AABB(in AABB2DInt shape, in Color color) => ShapeDraw.AABB(in shape, DrawData, in color);
        internal static void Polygon(in ReadOnlySpan<Vector2Int> shape, in Color color) => ShapeDraw.Polygon(in shape, DrawData, in color);

        internal static void Point(ref Vector2Int point) => ShapeDraw.Point(ref point, DrawData);
        internal static void Circle(ref Vector2Int point, ref int radius) => ShapeDraw.Circle(ref point, ref radius, DrawData);
        internal static void AABB(ref Vector2Int point, ref Vector2Int extents) => ShapeDraw.AABB(ref point, ref extents, DrawData);
        internal static void OBB(ref Vector2Int point, ref Vector2Int extents, ref float angle) => ShapeDraw.OBB(ref point, ref extents, ref angle, DrawData);
        internal static void Polygon(ref Vector2Int[] shape) => ShapeDraw.Polygon(ref shape, DrawData);

        internal static void Element(QuadTreeShape shape, int treeId, in QuadTreeElement element, bool drawIndex, in Color color)
        {
            if (!TryElement(shape, in element, drawIndex, in color))
                Debug.LogError($"TreeId:{treeId}, Shape:{shape}, not impl!");
        }
        private static bool TryElement(QuadTreeShape shape, in QuadTreeElement element, bool drawIndex, in Color color)
        {
            if (drawIndex)
                ShapeDraw.Label(element.Shape.Center(), DrawData, element.Index.ToString(), in color);

            switch (shape)
            {
                case QuadTreeShape.Circle:
                    ShapeDraw.Circle(Converts.AsCircleInt(in element.Shape), DrawData, in color);
                    return true;

                case QuadTreeShape.AABB:
                    ShapeDraw.AABB(in element.Shape, DrawData, in color);
                    return true;

                default: return false;
            }
        }

        internal static PropertyHandle EnumTreeFunc(this PropertyHandle handle, string path, bool disabled = false) => handle.DrawEnum(path, Proxy?.TreeEnum, disabled);
    }
}
#endif