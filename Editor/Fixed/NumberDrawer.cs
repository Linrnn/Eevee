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
            var size = new Vector2(position.width / 3, position.height);
            var rawValuePosition = new Rect(position.x, position.y, size.x * 2, size.y);
            var displayPosition = new Rect(position.x + size.x * 2, position.y, size.x, size.y);

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