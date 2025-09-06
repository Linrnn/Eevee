#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor
{
    internal static class EditorUtils
    {
        internal const string Script = "m_Script";
        internal const string X = "X";
        internal const string Y = "Y";
        internal const float SpaceWidth = 6;
        internal const float PropertyHeight = 18;

        internal static readonly GUIContent LabelX = new(X);
        internal static readonly GUIContent LabelY = new(Y);

        internal static Color RGBScale(this Color color, float scale) => new(color.r * scale, color.g * scale, color.b * scale, color.a);

        internal static float Diff(Vector2 vector, float value)
        {
            if (!Mathf.Approximately(vector.x, value))
                return vector.x;
            if (!Mathf.Approximately(vector.y, value))
                return vector.y;
            return value;
        }
        internal static float Diff(Vector3 vector)
        {
            if (Mathf.Approximately(vector.x, vector.y))
                return Math.Abs(vector.z);
            if (Mathf.Approximately(vector.x, vector.z))
                return Math.Abs(vector.y);
            if (Mathf.Approximately(vector.y, vector.z))
                return Math.Abs(vector.x);
            return Math.Abs(vector.x + vector.y + vector.z) / 3;
        }

        internal static void SetArrayLength(SerializedProperty property, int length)
        {
            while (property.arraySize < length)
                property.InsertArrayElementAtIndex(property.arraySize);
            while (property.arraySize > length)
                property.DeleteArrayElementAtIndex(property.arraySize - 1);
        }
        internal static float GetLabelWidth(GUIContent content)
        {
            var size = EditorStyles.label.CalcSize(content);
            return size.x;
        }
    }
}
#endif