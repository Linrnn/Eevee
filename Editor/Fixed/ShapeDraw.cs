#if UNITY_EDITOR
using Eevee.Fixed;
using System;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Fixed
{
    internal readonly struct ShapeDraw
    {
        private static GUIStyle _style;
        private static GUIStyle AllocStyle() => _style ??= new GUIStyle();

        internal static void Label(Vector2DInt point, float scale, float height, string label, in Color color)
        {
            var style = AllocStyle();
            var oldColor = style.normal.textColor;
            var oldAlignment = style.alignment;
            var position = AsVector3(point, scale, height);

            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(position, label, style);
            style.normal.textColor = oldColor;
            style.alignment = oldAlignment;
        }
        internal static void Point(Vector2DInt shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;
            float rad = 0;
            const int accuracy = 3;
            const float deltaRad = MathF.PI * 2 / accuracy;

            Handles.color = color;
            for (int i = 0; i < accuracy; ++i)
            {
                var p0 = new Vector2(MathF.Cos(rad), MathF.Sin(rad));
                rad += deltaRad;
                var p1 = new Vector2(MathF.Cos(rad), MathF.Sin(rad));
                Line(shape + p0, shape + p1, scale, height);
            }
            Handles.color = oldColor;
        }
        internal static void Circle(in CircleInt shape, int accuracy, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;
            float rad = 0;
            float deltaRad = MathF.PI * 2 / accuracy;

            Handles.color = color;
            for (int i = 0; i < accuracy; ++i)
            {
                var p0 = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * shape.R;
                rad += deltaRad;
                var p1 = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * shape.R;
                Line(shape.Center() + p0, shape.Center() + p1, scale, height);
            }
            Handles.color = oldColor;
        }
        internal static void AABB(in AABB2DInt shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;
            var lt = shape.LeftTop();
            var rt = shape.RightTop();
            var rb = shape.RightBottom();
            var lb = shape.LeftBottom();

            Handles.color = color;
            Line(lt, rt, scale, height);
            Line(rt, rb, scale, height);
            Line(rb, lb, scale, height);
            Line(lb, lt, scale, height);
            Handles.color = oldColor;
        }
        internal static void OBB(in OBB2DInt shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;
            shape.RotatedCorner(out var p0, out var p1, out var p2, out var p3);

            Handles.color = color;
            Line(in p0, in p1, scale, height);
            Line(in p1, in p2, scale, height);
            Line(in p2, in p3, scale, height);
            Line(in p3, in p0, scale, height);
            Handles.color = oldColor;
        }
        internal static void Polygon(in Span<Vector2Int> shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;

            Handles.color = color;
            for (int count = shape.Length, i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];
                Line(pi, pj, scale, height);
            }
            Handles.color = oldColor;
        }
        internal static void Polygon(in PolygonInt shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;

            Handles.color = color;
            for (int count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];
                Line(pi, pj, scale, height);
            }
            Handles.color = oldColor;
        }

        private static void Line(Vector2 p0, Vector2 p1, float scale, float height)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Handles.DrawLine(v0, v1);
        }
        private static void Line(in Vector2D p0, in Vector2D p1, float scale, float height)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Handles.DrawLine(v0, v1);
        }
        private static void Line(Vector2DInt p0, Vector2DInt p1, float scale, float height)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Handles.DrawLine(v0, v1);
        }

        private static Vector3 AsVector3(in Vector2 shape, float scale, float height) => new(shape.x * scale, height, shape.y * scale);
        private static Vector3 AsVector3(in Vector2D shape, float scale, float height) => new((float)shape.X * scale, height, (float)shape.Y * scale);
        private static Vector3 AsVector3(Vector2DInt shape, float scale, float height) => new(shape.X * scale, height, shape.Y * scale);
    }
}
#endif