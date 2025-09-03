#if UNITY_EDITOR
using Eevee.Fixed;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Fixed
{
    internal readonly struct NumberDrawer
    {
        internal static void OnGUI(in Rect position, SerializedProperty property, GUIContent label, string displayName)
        {
            const int scale = 4;
            var size = new Vector2(position.width / scale, position.height);
            float width = size.x * (scale - 1);
            var rawValuePosition = new Rect(position.x, position.y, width, size.y);
            var displayPosition = new Rect(position.x + width, position.y, size.x, size.y);

            var rawValueProperty = property.FindPropertyRelative(nameof(Fixed64.RawValue));
            long rawValue = EditorGUI.LongField(rawValuePosition, displayName, rawValueProperty.longValue);
            rawValueProperty.longValue = rawValue;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(displayPosition, new Fixed64(rawValue).ToString());
            EditorGUI.EndDisabledGroup();

            rawValueProperty.Dispose();
        }
    }
}
#endif