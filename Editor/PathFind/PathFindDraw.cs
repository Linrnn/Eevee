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
        private static Vector2 MinBoundary => Proxy.MinBoundary;
        private static float GridSize => Proxy.GridSize;
        private static float GridOffset => Proxy.GridOffset;

        internal static void Label(Vector2 point, in Color color, bool drawPoint, string ext = null)
        {
            var data = new DrawData(MinBoundary, GridSize / 2, GridSize, Height);
            if (drawPoint && !string.IsNullOrWhiteSpace(ext))
                ShapeDraw.Label(point, in data, $"{point}\nIndex:{ext}", in color);
            else if (drawPoint)
                ShapeDraw.Label(point, in data, point.ToString(), in color);
            else if (!string.IsNullOrWhiteSpace(ext))
                ShapeDraw.Label(point, in data, ext, in color);
        }
        internal static void Line(Vector2 lhs, Vector2 rhs, in Color color)
        {
            var data = new DrawData(MinBoundary, GridOffset * GridSize, GridSize, Height);
            ShapeDraw.Line(lhs, rhs, in data, in color);
        }
        internal static void Grid(Vector2 point, in Color color)
        {
            var data = new DrawData(MinBoundary, GridOffset * GridSize, GridSize, Height);
            ShapeDraw.AABB(point, _extents, in data, in color);
        }
        internal static void Grid(Vector2 point, float decrease, in Color color)
        {
            var data = new DrawData(MinBoundary, GridOffset * GridSize, GridSize, Height);
            ShapeDraw.AABB(point, decrease * _extents, in data, in color);
        }
        internal static void Side(Vector2 point, Vector2 dir, in Color color)
        {
            const int scale = 2;
            var side = point + dir / scale;
            var sub = new Vector2(dir.y, dir.x) / scale;
            var lhs = side + sub;
            var rhs = side - sub;
            var data = new DrawData(MinBoundary, GridOffset * GridSize, GridSize, Height);
            ShapeDraw.Line(lhs, rhs, in data, in color);
        }
        internal static void Arrow(Vector2 point, Vector2 dir, in Color color)
        {
            var length = point - 0.75F * dir;
            var width = 0.25F * dir;
            var lhs = new Vector2(length.x + width.y, length.y - width.x);
            var rhs = new Vector2(length.x - width.y, length.y + width.x);
            var data = new DrawData(MinBoundary, GridOffset * GridSize, GridSize, Height);
            ShapeDraw.Line(point, lhs, in data, in color);
            ShapeDraw.Line(point, rhs, in data, in color);
        }
        internal static void ObliqueArrow(Vector2 point, Vector2 dir, in Color color)
        {
            const float sideScale = 5F / 8;
            var halfDir = 3F / 8 * dir;
            var mhs = point + halfDir;
            var lhs = new Vector2(point.x - halfDir.x * sideScale, point.y - halfDir.y);
            var rhs = new Vector2(point.x - halfDir.x, point.y - halfDir.y * sideScale);
            var data = new DrawData(MinBoundary, GridOffset * GridSize, GridSize, Height);
            ShapeDraw.Line(mhs, lhs, in data, in color);
            ShapeDraw.Line(mhs, rhs, in data, in color);
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