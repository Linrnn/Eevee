#if UNITY_EDITOR
using Eevee.Fixed;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    internal static class EditorHelper
    {
        internal static void DrawRect(in AABB2DInt aabb, in Color color, float height, float lineDuration)
        {
            var lt = aabb.LeftTop();
            var rt = aabb.RightTop();
            var rb = aabb.RightBottom();
            var lb = aabb.LeftBottom();

            DrawLine(lt, rt, in color, height, lineDuration);
            DrawLine(rt, rb, in color, height, lineDuration);
            DrawLine(rb, lb, in color, height, lineDuration);
            DrawLine(lb, lt, in color, height, lineDuration);
        }
        internal static void DrawLine(Vector2Int point0, Vector2Int point1, in Color color, float height, float lineDuration)
        {
            var p0 = ConvertVector(point0, height);
            var p1 = ConvertVector(point1, height);

            Debug.DrawLine(p0, p1, color, lineDuration);
        }
        internal static Vector3 ConvertVector(Vector2Int vec, float height) => new(vec.x, height, vec.y);
    }
}
#endif