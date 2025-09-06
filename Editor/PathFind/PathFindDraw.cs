#if UNITY_EDITOR
using EeveeEditor.Fixed;
using UnityEngine;

namespace EeveeEditor.PathFind
{
    internal static class PathFindDraw
    {
        internal static float Height;
        private static readonly Vector2 _extents = new(0.5F, 0.5F);
        private static IPathFindDrawProxy Proxy => PathFindGetter.Proxy;
        private static float Offset => Proxy.GridOffset;

        internal static void Label(float x, float y, float scale, Vector2 offset, in Color color, bool drawPoint, string ext = null)
        {
            var position = new Vector2(x, y);
            var data = new DrawData(offset, scale / 2, scale, Height);
            if (drawPoint && !string.IsNullOrWhiteSpace(ext))
                ShapeDraw.Label(position, in data, $"({x}, {y})\nIndex:{ext}", in color);
            else if (drawPoint)
                ShapeDraw.Label(position, in data, $"({x}, {y})", in color);
            else if (!string.IsNullOrWhiteSpace(ext))
                ShapeDraw.Label(position, in data, ext, in color);
        }

        internal static void Grid(float x, float y, float scale, Vector2 offset, in Color color)
        {
            var data = new DrawData(offset, Offset * scale, scale, Height);
            ShapeDraw.AABB(new Vector2(x, y), _extents, in data, in color);
        }
        internal static void Grid(float x, float y, float pointScale, float sizeScale, Vector2 offset, in Color color)
        {
            var data = new DrawData(offset, Offset * pointScale, pointScale, Height);
            ShapeDraw.AABB(new Vector2(x, y), sizeScale / pointScale * _extents, in data, in color);
        }

        internal static void Side(float x, float y, Vector2 dir, float scale, Vector2 offset, in Color color)
        {
            var lhs = new Vector2(x, y) + new Vector2(dir.x + dir.y, dir.y + dir.x) / 2;
            var rhs = new Vector2(x, y) + new Vector2(dir.x - dir.y, dir.y - dir.x) / 2;
            var data = new DrawData(offset, Offset * scale, scale, Height);
            ShapeDraw.Line(lhs, rhs, in data, in color);
        }
        internal static void Arrow(float x, float y, Vector2 dir, float scale, Vector2 offset, in Color color)
        {
            var length = 0.75F * dir;
            var width = 0.25F * dir;
            var lhs = new Vector2(x - length.x + width.y, y - length.y - width.x);
            var rhs = new Vector2(x - length.x - width.y, y - length.y + width.x);
            var data = new DrawData(offset, Offset * scale, scale, Height);
            ShapeDraw.Line(new Vector2(x, y), lhs, in data, in color);
            ShapeDraw.Line(new Vector2(x, y), rhs, in data, in color);
        }
        internal static void ObliqueArrow(float x, float y, Vector2 dir, float scale, Vector2 offset, in Color color)
        {
            const float sideScale = 5F / 8;
            var halfDir = 3F / 8 * dir;
            var mhs = new Vector2(x + halfDir.x, y + halfDir.y);
            var lhs = new Vector2(x - halfDir.x * sideScale, y - halfDir.y);
            var rhs = new Vector2(x - halfDir.x, y - halfDir.y * sideScale);
            var data = new DrawData(offset, Offset * scale, scale, Height);
            ShapeDraw.Line(mhs, lhs, in data, in color);
            ShapeDraw.Line(mhs, rhs, in data, in color);
        }
        internal static void Line(float sx, float sy, float ex, float ey, float scale, Vector2 offset, in Color color)
        {
            var data = new DrawData(offset, Offset * scale, scale, Height);
            ShapeDraw.Line(new Vector2(sx, sy), new Vector2(ex, ey), in data, in color);
        }

        internal static PropertyHandle EnumGroupType(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, Proxy?.GroupTypeEnum, disabled);
        }
        internal static PropertyHandle EnumMoveType(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, Proxy?.MoveTypeEnum, disabled);
        }
        internal static PropertyHandle EnumCollType(this PropertyHandle handle, string path, bool disabled = false)
        {
            return handle.DrawEnum(path, Proxy?.CollTypeEnum, disabled);
        }
    }
}
#endif