#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor
{
    internal readonly struct EditorUtils
    {
        internal const string Script = "m_Script";

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
    }
}
#endif