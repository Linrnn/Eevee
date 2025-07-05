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
        internal static void Label(Vector2DInt point, float scale, float height, string label, in Color color)
        {
            var style = GUI.skin.label;
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
            var center = AsVector3(shape, scale, height);

            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, scale);
            Handles.color = oldColor;
        }
        internal static void Circle(in CircleInt shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(shape.Center(), scale, height);

            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.up, shape.R * scale);
            Handles.color = oldColor;
        }
        internal static void AABB(in AABB2DInt shape, float scale, float height, in Color color)
        {
            var oldColor = Handles.color;
            var center = AsVector3(shape.Center(), scale, height);
            var size = AsVector3(shape.Size(), scale, 0);

            Handles.color = color;
            Handles.DrawWireCube(center, size);
            Handles.color = oldColor;
        }
        internal static void OBB(in OBB2DInt shape, float scale, float height, in Color color)
        {
            var oldMatrix = Handles.matrix;
            var oldColor = Handles.color;
            var center = AsVector3(shape.Center(), scale, height);
            var rotation = UQuaternion.Euler(0, -(float)shape.A.Value, 0);
            var size = AsVector3(shape.Size(), scale, 0);

            Handles.matrix = Matrix4x4.TRS(center, rotation, size);
            Handles.color = color;
            Handles.DrawWireCube(default, Vector3.one);
            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }
        internal static void Polygon(in ReadOnlySpan<Vector2Int> shape, float scale, float height, in Color color)
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

        internal static void Point(ref Vector2Int point, float scale, float height)
        {
            var center = AsVector3(point, scale, height);

            point = AsVector2Int(Handles.PositionHandle(center, UQuaternion.identity), scale);
        }
        internal static void Circle(ref Vector2Int point, ref int radius, float scale, float height)
        {
            var center = AsVector3(point, scale, height);
            float rad = radius * scale;
            var size = new Vector3(rad, 0, rad);

            Handles.TransformHandle(ref center, UQuaternion.identity, ref size);
            point = AsVector2Int(center, scale);
            radius = (int)(EditorUtils.Diff(new Vector2(size.x, size.z), rad) / scale);
        }
        internal static void AABB(ref Vector2Int point, ref Vector2Int extents, float scale, float height)
        {
            var center = AsVector3(point, scale, height);
            var size = AsVector3(extents, scale, 0);

            Handles.TransformHandle(ref center, UQuaternion.identity, ref size);
            point = AsVector2Int(center, scale);
            extents = AsVector2Int(size, scale);
        }
        internal static void OBB(ref Vector2Int point, ref Vector2Int extents, ref float angle, float scale, float height)
        {
            var center = AsVector3(point, scale, height);
            var rotation = UQuaternion.Euler(0, -angle, 0);
            var size = AsVector3(extents, scale, 0);

            Handles.TransformHandle(ref center, ref rotation, ref size);
            point = AsVector2Int(center, scale);
            angle = (-rotation.eulerAngles.y % 360 + 360) % 360;
            extents = AsVector2Int(size, scale);
        }
        internal static void Polygon(ref Vector2Int[] shape, float scale, float height)
        {
            for (int length = shape.Length, i = 0; i < length; ++i)
            {
                ref var point = ref shape[i];
                var center = AsVector3(point, scale, height);
                point = AsVector2Int(Handles.PositionHandle(center, UQuaternion.identity), scale);
            }
        }

        private static void Line(Vector2DInt p0, Vector2DInt p1, float scale, float height)
        {
            var v0 = AsVector3(p0, scale, height);
            var v1 = AsVector3(p1, scale, height);
            Handles.DrawLine(v0, v1);
        }
        private static Vector3 AsVector3(Vector2DInt vector, float scale, float height) => new(vector.X * scale, height, vector.Y * scale);
        private static Vector2DInt AsVector2Int(Vector3 vector, float scale) => new((int)(vector.x / scale), (int)(vector.z / scale));
    }
}
#endif