#if UNITY_EDITOR
using Eevee.Fixed;
using Eevee.QuadTree;
using EeveeEditor.Fixed;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal static class QuadTreeDraw
    {
        internal static float Height;

        internal static void Element(QuadTreeShape shape, in QuadTreeElement element, float scale, bool drawIndex, in Color color)
        {
            if (!TryElement(shape, in element, scale, drawIndex, in color))
                Debug.LogError($"[Editor][Quad] Shape:{shape}, not impl!");
        }
        internal static void Element(QuadTreeShape shape, int treeId, in QuadTreeElement element, float scale, bool drawIndex, in Color color)
        {
            if (!TryElement(shape, in element, scale, drawIndex, in color))
                Debug.LogError($"[Editor][Quad] TreeId:{treeId}, Shape:{shape}, not impl!");
        }
        private static bool TryElement(QuadTreeShape shape, in QuadTreeElement element, float scale, bool drawIndex, in Color color)
        {
            var drawData = new DrawData(scale, Height);
            if (drawIndex)
                ShapeDraw.Label(element.Shape.Center(), in drawData, element.Index.ToString(), in color);

            switch (shape)
            {
                case QuadTreeShape.Circle:
                    ShapeDraw.Circle(Converts.AsCircleInt(in element.Shape), in drawData, in color);
                    return true;

                case QuadTreeShape.AABB:
                    ShapeDraw.AABB(in element.Shape, in drawData, in color);
                    return true;

                default: return false;
            }
        }

        internal static PropertyHandle EnumTreeFunc(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, QuadTreeGetter.Proxy?.TreeEnum, disabled);
        }
    }
}
#endif