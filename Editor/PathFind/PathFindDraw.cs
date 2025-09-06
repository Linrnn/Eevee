#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    internal static class PathFindDraw
    {
        internal static float Height;

        internal static void Text(float x, float y, float scale, Vector2 offset, in Color color, bool drawPoint, string ext = null)
        {
            Count(x, y, scale, offset, out var center);
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

        internal static void Grid(int x, int y, float scale, Vector2 offset, in Color color)
        {
            Count(x, y, scale, scale, offset, out var center, out var size);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }
        internal static void Grid(float x, float y, float positionScale, float sizeScale, Vector2 offset, in Color color)
        {
            Count(x, y, positionScale, sizeScale, offset, out var center, out var size);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }

        internal static void Side(int x, int y, Vector2 dir, float scale, Vector2 offset, in Color color)
        {
            Count(x, y, scale, offset, out var center);
            float halfSize = scale * 0.5F;
            var lhs = halfSize * new Vector3(dir.x + dir.y, 0, dir.y + dir.x);
            var rhs = halfSize * new Vector3(dir.x - dir.y, 0, dir.y - dir.x);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawLine(center + lhs, center + rhs);
            Handles.color = oldColor;
        }
        internal static void Arrow(float x, float y, Vector2 dir, float scale, Vector2 offset, in Color color)
        {
            Count(x, y, scale, offset, out var center);
            var length = 0.75F * scale * dir;
            var width = 0.25F * scale * dir;
            var oldColor = Handles.color;
            var point0 = new Vector3(center.x - length.x - width.y, center.y, center.z - length.y + width.x);
            var point1 = new Vector3(center.x - length.x + width.y, center.y, center.z - length.y - width.x);

            Handles.color = color;
            Handles.DrawLine(center, point0);
            Handles.DrawLine(center, point1);
            Handles.color = oldColor;
        }
        internal static void ObliqueArrow(float x, float y, Vector2 dir, float scale, Vector2 offset, in Color color)
        {
            Count(x, y, scale, scale * 0.4F, offset, out var center, out var size);
            var sideOffset = new Vector3(dir.x * size.x, size.y, dir.y * size.z);
            var oldColor = Handles.color;
            var point0 = center + sideOffset;
            var point1 = center - sideOffset + new Vector3(dir.x * size.x * 0.5F, 0);
            var point2 = center - sideOffset + new Vector3(0, dir.y * size.z * 0.5F);

            Handles.color = color;
            Handles.DrawLine(point0, point1);
            Handles.DrawLine(point0, point2);
            Handles.color = oldColor;
        }
        internal static void Line(float sx, float sy, float ex, float ey, float scale, Vector2 offset, in Color color)
        {
            Count(sx, sy, scale, offset, out var sc);
            Count(ex, ey, scale, offset, out var ec);
            var oldColor = Handles.color;

            Handles.color = color;
            Handles.DrawLine(sc, ec);
            Handles.color = oldColor;
        }

        private static void Count(float x, float y, float scale, Vector2 offset, out Vector3 center)
        {
            float halfSize = scale * 0.5F;
            float cx = x * scale + offset.x;
            float cz = y * scale + offset.y;

            center = new Vector3(cx + halfSize, Height, cz + halfSize);
        }
        private static void Count(float x, float y, float positionScale, float sizeScale, Vector2 offset, out Vector3 center, out Vector3 size)
        {
            float halfSize = positionScale * 0.5F;
            float cx = x * positionScale + offset.x;
            float cz = y * positionScale + offset.y;

            center = new Vector3(cx + halfSize, Height, cz + halfSize);
            size = new Vector3(sizeScale, 0, sizeScale);
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