#if UNITY_EDITOR
using Eevee.Fixed;
using System;
using UnityEditor;
using UnityEngine;
using UQuaternion = UnityEngine.Quaternion;

namespace EeveeEditor.Fixed
{
    internal readonly struct ShapeDraw
    {
        internal static void Label(Vector2DInt point, in DrawData draw, string label, in Color color)
        {
            var style = GUI.skin.label;
            var oldColor = style.normal.textColor;
            var oldAlignment = style.alignment;
            var position = AsVector3(point, in draw);

            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(position, label, style);
            style.normal.textColor = oldColor;
            style.alignment = oldAlignment;
        }
        internal static void Point(Vector2DInt shape, in DrawData draw, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(shape, in draw);

            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, draw.Scale);
            Handles.color = oldColor;
        }
        internal static void Circle(in CircleInt shape, in DrawData draw, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(shape.Center(), in draw);

            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, shape.R * draw.Scale);
            Handles.color = oldColor;
        }
        internal static void AABB(in AABB2DInt shape, in DrawData draw, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(shape.Center(), in draw);
            var size = AsVector3(shape.Size(), in draw);
            size.y = default;

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }
        internal static void OBB(in OBB2DInt shape, in DrawData draw, in Color color)
        {
            var oldMatrix = Handles.matrix;
            var oldColor = Handles.color;
            var center = AsVector3(shape.Center(), in draw);
            var rotation = UQuaternion.Euler(0, -(float)shape.A.Value, 0);
            var size = AsVector3(shape.Size(), in draw);
            size.y = default;

            Handles.matrix = Matrix4x4.TRS(center, rotation, size);
            Handles.color = color;
            Handles.DrawWireCube(default, Vector3.one);
            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }
        internal static void Polygon(in ReadOnlySpan<Vector2Int> shape, in DrawData draw, in Color color)
        {
            var oldColor = Handles.color;

            Handles.color = color;
            for (int count = shape.Length, i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];
                Line(pi, pj, in draw);
            }
            Handles.color = oldColor;
        }

        internal static void Point(ref Vector2Int point, in DrawData draw)
        {
            var center = AsVector3(point, in draw);

            point = AsVector2Int(Handles.PositionHandle(center, UQuaternion.identity), in draw);
        }
        internal static void Circle(ref Vector2Int point, ref int radius, in DrawData draw)
        {
            var center = AsVector3(point, in draw);
            float rad = radius * draw.Scale;
            var size = new Vector3(rad, 0, rad);

            Handles.TransformHandle(ref center, UQuaternion.identity, ref size);
            point = AsVector2Int(center, in draw);
            radius = (int)(EditorUtils.Diff(new Vector2(size.x, size.z), rad) / draw.Scale);
        }
        internal static void AABB(ref Vector2Int point, ref Vector2Int extents, in DrawData draw)
        {
            var center = AsVector3(point, in draw);
            var size = AsVector3(extents, in draw);
            size.y = default;

            Handles.TransformHandle(ref center, UQuaternion.identity, ref size);
            point = AsVector2Int(center, in draw);
            extents = AsVector2Int(size, in draw);
        }
        internal static void OBB(ref Vector2Int point, ref Vector2Int extents, ref float angle, in DrawData draw)
        {
            var center = AsVector3(point, in draw);
            var rotation = UQuaternion.Euler(0, -angle, 0);
            var size = AsVector3(extents, in draw);
            size.y = default;

            Handles.TransformHandle(ref center, ref rotation, ref size);
            point = AsVector2Int(center, in draw);
            angle = (-rotation.eulerAngles.y % 360 + 360) % 360;
            extents = AsVector2Int(size, in draw);
        }
        internal static void Polygon(ref Vector2Int[] shape, in DrawData draw)
        {
            for (int length = shape.Length, i = 0; i < length; ++i)
            {
                ref var point = ref shape[i];
                var center = AsVector3(point, in draw);
                point = AsVector2Int(Handles.PositionHandle(center, UQuaternion.identity), in draw);
            }
        }

        private static void Line(Vector2DInt lhs, Vector2DInt rhs, in DrawData draw)
        {
            var v0 = AsVector3(lhs, in draw);
            var v1 = AsVector3(rhs, in draw);
            Handles.DrawLine(v0, v1);
        }
        private static Vector3 AsVector3(Vector2DInt vector, in DrawData draw) => new(vector.X * draw.Scale + draw.OffsetX, draw.Height, vector.Y * draw.Scale + draw.OffsetY);
        private static Vector2DInt AsVector2Int(Vector3 vector, in DrawData draw) => new((int)((vector.x - draw.OffsetX) / draw.Scale), (int)((vector.z - draw.OffsetY) / draw.Scale));
    }
}
#endif