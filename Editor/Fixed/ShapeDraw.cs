#if UNITY_EDITOR
using Eevee.Fixed;
using System;
using UnityEngine;

namespace EeveeEditor.Fixed
{
    internal readonly struct ShapeDraw
    {
        internal static void Circle(in CircleInt shape, int pointCount, float scale, float height, in Color color, float duration)
        {
            float rad = 0;
            float deltaRad = MathF.PI * 2 / pointCount;
            for (int i = 0; i < pointCount; ++i)
            {
                var p0 = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * shape.R;
                rad += deltaRad;
                var p1 = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * shape.R;
                Line(shape.Center() + p0, shape.Center() + p1, scale, height, in color, duration);
            }
        }
        internal static void AABB(in AABB2DInt shape, float scale, float height, in Color color, float duration)
        {
            var lt = shape.LeftTop();
            var rt = shape.RightTop();
            var rb = shape.RightBottom();
            var lb = shape.LeftBottom();
            Line(lt, rt, scale, height, in color, duration);
            Line(rt, rb, scale, height, in color, duration);
            Line(rb, lb, scale, height, in color, duration);
            Line(lb, lt, scale, height, in color, duration);
        }
        internal static void OBB(in OBB2DInt shape, float scale, float height, in Color color, float duration)
        {
            shape.RotatedCorner(out var p0, out var p1, out var p2, out var p3);
            Line(in p0, in p1, scale, height, in color, duration);
            Line(in p1, in p2, scale, height, in color, duration);
            Line(in p2, in p3, scale, height, in color, duration);
            Line(in p3, in p0, scale, height, in color, duration);
        }
        internal static void OBB(in PolygonInt shape, float scale, float height, in Color color, float duration)
        {
            for (int count = shape.PointCount(), i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];
                Line(pi, pj, scale, height, in color, duration);
            }
        }

        private static void Line(Vector2 p0, Vector2 p1, float scale, float height, in Color color, float duration)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Debug.DrawLine(v0, v1, color, duration);
        }
        private static void Line(in Vector2D p0, in Vector2D p1, float scale, float height, in Color color, float duration)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Debug.DrawLine(v0, v1, color, duration);
        }
        private static void Line(Vector2DInt p0, Vector2DInt p1, float scale, float height, in Color color, float duration)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Debug.DrawLine(v0, v1, color, duration);
        }
        private static Vector3 AsVector3(in Vector2 shape, float scale, float height) => new(shape.x * scale, height, shape.y * scale);
        private static Vector3 AsVector3(in Vector2D shape, float scale, float height) => new((float)shape.X * scale, height, (float)shape.Y * scale);
        private static Vector3 AsVector3(Vector2DInt shape, float scale, float height) => new(shape.X * scale, height, shape.Y * scale);
    }
}
#endif