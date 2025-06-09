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
            float width = position.width / 3;
            var rawValuePosition = new Rect(position.x, position.y, width * 2, position.height);
            var displayPosition = new Rect(position.x + width * 2, position.y, width, position.height);

            var rawValueProperty = property.FindPropertyRelative(nameof(Fixed64.RawValue));
            rawValueProperty.longValue = EditorGUI.LongField(rawValuePosition, displayName, rawValueProperty.longValue);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(displayPosition, label, new Fixed64(rawValueProperty.longValue).ToString());
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif