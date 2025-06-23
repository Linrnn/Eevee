#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.QuadTree
{
    [Serializable]
    internal struct ColorSetting
    {
        [SerializeField] internal bool Show;
        [SerializeField] internal Color Color;
        internal ColorSetting(bool show, in Color color)
        {
            Show = show;
            Color = color;
        }
    }

    [CustomPropertyDrawer(typeof(ColorSetting))]
    internal sealed class ColorSettingDrawer : PropertyDrawer
    {
        private const int HeightScale = 2;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var size = new Vector2(position.size.x, position.size.y / HeightScale);
            var showPosition = new Rect(position.position, size);
            var colorPosition = new Rect(position.x, position.y + size.y, size.x, size.y);

            var showProperty = property.FindPropertyRelative(nameof(ColorSetting.Show));
            var colorProperty = property.FindPropertyRelative(nameof(ColorSetting.Color));

            EditorGUILayout.BeginHorizontal();
            showProperty.boolValue = EditorGUI.Toggle(showPosition, label, showProperty.boolValue);
            ++EditorGUI.indentLevel;
            EditorGUI.PropertyField(colorPosition, colorProperty);
            --EditorGUI.indentLevel;
            EditorGUILayout.EndHorizontal();

            showProperty.Dispose();
            colorProperty.Dispose();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label) * HeightScale;
    }
}
#endif