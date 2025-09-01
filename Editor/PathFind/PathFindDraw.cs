#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    internal static class PathFindDraw
    {
        internal static float Height;

        internal static void Text(float x, float y, float gridSize, Vector2 minBoundary, in Color color, bool drawPoint, string ext = null)
        {
            CountCenter(x, y, gridSize, minBoundary, out var center);
            var style = GUI.skin.label;
            var oldColor = style.normal.textColor;
            var oldAlignment = style.alignment;
            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;

            if (drawPoint && !string.IsNullOrWhiteSpace(ext))
                Handles.Label(center, $"({x}, {y})\n{ext}", style);
            else if (drawPoint)
                Handles.Label(center, $"({x}, {y})", style);
            else if (!string.IsNullOrWhiteSpace(ext))
                Handles.Label(center, ext, style);

            style.normal.textColor = oldColor;
            style.alignment = oldAlignment;
        }

        internal static void Grid(int x, int y, float gridSize, Vector2 minBoundary, in Color color)
        {
            CountCenterSize(x, y, gridSize, gridSize, minBoundary, out var center, out var size);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }
        internal static void Grid(float x, float y, float positionSize, float scaleSize, Vector2 minBoundary, in Color color)
        {
            CountCenterSize(x, y, positionSize, scaleSize, minBoundary, out var center, out var size);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }

        internal static void Side(int x, int y, Vector2 dir, float gridSize, Vector2 minBoundary, in Color color)
        {
            CountCenter(x, y, gridSize, minBoundary, out var center);
            float halfSize = gridSize * 0.5F;
            var lhs = halfSize * new Vector3(dir.x + dir.y, 0, dir.y + dir.x);
            var rhs = halfSize * new Vector3(dir.x - dir.y, 0, dir.y - dir.x);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawLine(center + lhs, center + rhs);
            Handles.color = oldColor;
        }
        internal static void Arrow(float x, float y, Vector2 dir, float gridSize, Vector2 minBoundary, in Color color)
        {
            CountCenter(x, y, gridSize, minBoundary, out var center);
            var length = 0.75F * gridSize * dir;
            var width = 0.25F * gridSize * dir;
            var oldColor = Handles.color;
            var point0 = new Vector3(center.x - length.x - width.y, center.y, center.z - length.y + width.x);
            var point1 = new Vector3(center.x - length.x + width.y, center.y, center.z - length.y - width.x);

            Handles.color = color;
            Handles.DrawLine(center, point0);
            Handles.DrawLine(center, point1);
            Handles.color = oldColor;
        }
        internal static void ObliqueArrow(float x, float y, Vector2 dir, float gridSize, Vector2 minBoundary, in Color color)
        {
            CountCenterSize(x, y, gridSize, gridSize * 0.4F, minBoundary, out var center, out var size);
            var offset = new Vector3(dir.x * size.x, size.y, dir.y * size.z);
            var oldColor = Handles.color;
            var point0 = center + offset;
            var point1 = center - offset + new Vector3(dir.x * size.x * 0.5F, 0);
            var point2 = center - offset + new Vector3(0, dir.y * size.z * 0.5F);

            Handles.color = color;
            Handles.DrawLine(point0, point1);
            Handles.DrawLine(point0, point2);
            Handles.color = oldColor;
        }
        internal static void Line(float sx, float sy, float ex, float ey, float gridSize, Vector2 minBoundary, in Color color)
        {
            CountCenter(sx, sy, gridSize, minBoundary, out var sc);
            CountCenter(ex, ey, gridSize, minBoundary, out var ec);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawLine(sc, ec);
            Handles.color = oldColor;
        }

        private static void CountCenter(float x, float y, float size, Vector2 minBoundary, out Vector3 center)
        {
            float halfSize = size * 0.5F;
            float xCenter = x * size + minBoundary.x;
            float zCenter = y * size + minBoundary.y;

            center = new Vector3(xCenter + halfSize, Height, zCenter + halfSize);
        }
        private static void CountCenterSize(float x, float y, float positionSize, float scaleSize, Vector2 minBoundary, out Vector3 center, out Vector3 size)
        {
            float halfSize = positionSize * 0.5F;
            float xCenter = x * positionSize + minBoundary.x;
            float zCenter = y * positionSize + minBoundary.y;

            center = new Vector3(xCenter + halfSize, Height, zCenter + halfSize);
            size = new Vector3(scaleSize, 0, scaleSize);
        }

        internal static PropertyHandle EnumGroupType(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, PathFindGetter.Proxy?.GroupTypeEnum, disabled);
        }
        internal static PropertyHandle EnumMoveType(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, PathFindGetter.Proxy?.MoveTypeEnum, disabled);
        }
        internal static PropertyHandle EnumCollType(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, PathFindGetter.Proxy?.CollTypeEnum, disabled);
        }
    }
}
#endif