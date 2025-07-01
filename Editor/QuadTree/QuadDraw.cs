#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal readonly struct QuadDraw
    {
        internal static void Element(QuadShape shape, in QuadElement element, float scale, float height, in Color color)
        {
            if (!TryElement(shape, in element, scale, height, in color))
                Debug.LogError($"[Editor][Quad] Shape:{shape}, not impl!");
        }
        internal static void Element(QuadShape shape, int treeId, in QuadElement element, float scale, float height, in Color color)
        {
            if (!TryElement(shape, in element, scale, height, in color))
                Debug.LogError($"[Editor][Quad] TreeId:{treeId}, Shape:{shape}, not impl!");
        }
        private static bool TryElement(QuadShape shape, in QuadElement element, float scale, float height, in Color color)
        {
            ShapeDraw.Label(element.Shape.Center(), scale, height, element.Index.ToString(), in color);
            switch (shape)
            {
                case QuadShape.Circle:
                    ShapeDraw.Circle(Converts.AsCircleInt(in element.Shape), scale, height, in color);
                    return true;

                case QuadShape.AABB:
                    ShapeDraw.AABB(in element.Shape, scale, height, in color);
                    return true;

                default: return false;
            }
        }
    }
}
#endif