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
        internal static void Label(Vector2 point, in DrawData data, string label, in Color color)
        {
            var style = GUI.skin.label;
            var oldColor = style.normal.textColor;
            var oldAlignment = style.alignment;
            var position = AsVector3(point, in data);

            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(position, label, style);
            style.normal.textColor = oldColor;
            style.alignment = oldAlignment;
        }
        internal static void Line(Vector2 lhs, Vector2 rhs, in DrawData data, in Color color)
        {
            var oldColor = Handles.color;

            Handles.color = color;
            Line(lhs, rhs, in data);
            Handles.color = oldColor;
        }
        internal static void Point(Vector2 shape, in DrawData data, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(shape, in data);

            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, data.Scale);
            Handles.color = oldColor;
        }
        internal static void Circle(Vector2 point, float radius, in DrawData data, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(point, in data);

            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, radius * data.Scale);
            Handles.color = oldColor;
        }
        internal static void AABB(Vector2 point, Vector2 extents, in DrawData data, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(point, in data);
            var size = AsVector3(extents * 2, data.OnlyScale());

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }
        internal static void OBB(Vector2 point, Vector2 extents, float angle, in DrawData data, in Color color)
        {
            var oldMatrix = Handles.matrix;
            var oldColor = Handles.color;
            var center = AsVector3(point, in data);
            var rotation = UQuaternion.Euler(0, -angle, 0);
            var size = AsVector3(extents * 2, data.OnlyScale());

            Handles.matrix = Matrix4x4.TRS(center, rotation, size);
            Handles.color = color;
            Handles.DrawWireCube(default, Vector3.one);
            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }

        internal static void Circle(in CircleInt shape, in DrawData data, in Color color) => Circle(shape.Center(), shape.R, in data, in color);
        internal static void AABB(in AABB2DInt shape, in DrawData data, in Color color) => AABB(shape.Center(), shape.HalfSize(), in data, in color);
        internal static void OBB(in OBB2DInt shape, in DrawData data, in Color color) => OBB(shape.Center(), shape.HalfSize(), (float)shape.A.Value, in data, in color);
        internal static void Polygon(in ReadOnlySpan<Vector2Int> shape, in DrawData data, in Color color)
        {
            var oldColor = Handles.color;

            Handles.color = color;
            for (int count = shape.Length, i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = shape[i];
                var pj = shape[j];
                Line(pi, pj, in data);
            }
            Handles.color = oldColor;
        }

        internal static void Point(ref Vector2Int point, in DrawData data)
        {
            var center = AsVector3(point, in data);

            point = AsVector2Int(Handles.PositionHandle(center, UQuaternion.identity), in data);
        }
        internal static void Circle(ref Vector2Int point, ref int radius, in DrawData data)
        {
            var center = AsVector3(point, in data);
            float rad = radius * data.Scale;
            var size = new Vector3(rad, 0, rad);

            Handles.TransformHandle(ref center, UQuaternion.identity, ref size);
            point = AsVector2Int(in center, in data);
            radius = (int)(EditorUtils.Diff(new Vector2(size.x, size.z), rad) / data.Scale);
        }
        internal static void AABB(ref Vector2Int point, ref Vector2Int extents, in DrawData data)
        {
            var center = AsVector3(point, in data);
            var size = AsVector3(extents, data.OnlyScale());

            Handles.TransformHandle(ref center, UQuaternion.identity, ref size);
            point = AsVector2Int(in center, in data);
            extents = AsVector2Int(in size, in data);
        }
        internal static void OBB(ref Vector2Int point, ref Vector2Int extents, ref float angle, in DrawData data)
        {
            var center = AsVector3(point, in data);
            var rotation = UQuaternion.Euler(0, -angle, 0);
            var size = AsVector3(extents, data.OnlyScale());

            Handles.TransformHandle(ref center, ref rotation, ref size);
            point = AsVector2Int(in center, in data);
            angle = (-rotation.eulerAngles.y % 360 + 360) % 360;
            extents = AsVector2Int(in size, in data);
        }
        internal static void Polygon(ref Vector2Int[] shape, in DrawData data)
        {
            for (int length = shape.Length, i = 0; i < length; ++i)
            {
                ref var point = ref shape[i];
                var center = AsVector3(point, in data);
                point = AsVector2Int(Handles.PositionHandle(center, UQuaternion.identity), in data);
            }
        }

        private static void Line(Vector2 lhs, Vector2 rhs, in DrawData data)
        {
            var v0 = AsVector3(lhs, in data);
            var v1 = AsVector3(rhs, in data);
            Handles.DrawLine(v0, v1);
        }
        private static Vector3 AsVector3(Vector2 value, in DrawData data)
        {
            var vector = value * data.Scale + data.GetOffset();
            return new Vector3(vector.x, data.Height, vector.y);
        }
        private static Vector2DInt AsVector2Int(in Vector3 value, in DrawData data)
        {
            var vector = new Vector2(value.x, value.z) - data.GetOffset();
            return (Vector2DInt)(vector / data.Scale);
        }
    }
}
#endif