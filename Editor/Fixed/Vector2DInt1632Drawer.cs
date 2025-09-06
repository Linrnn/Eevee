#if UNITY_EDITOR
using Eevee.Fixed;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Fixed
{
    [CustomPropertyDrawer(typeof(Vector2DInt16))]
    [CustomPropertyDrawer(typeof(Vector2DInt))]
    internal sealed class Vector2DInt1632Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const int scale = 2;
            const float spaceWidth = EditorUtils.SpaceWidth / 2;

            var min = position.position;
            float labelWidth = EditorGUIUtility.labelWidth;
            float width = (position.size.x - labelWidth) / scale;

            var xProperty = property.FindPropertyRelative(EditorUtils.X);
            var yProperty = property.FindPropertyRelative(EditorUtils.Y);
            EditorGUI.LabelField(new Rect(min, new Vector2(labelWidth, EditorUtils.PropertyHeight)), label);

            EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(EditorUtils.LabelX);
            xProperty.intValue = EditorGUI.IntField(new Rect(min.x + labelWidth, min.y, width - spaceWidth, 18), xProperty.displayName, xProperty.intValue);
            EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(EditorUtils.LabelY);
            yProperty.intValue = EditorGUI.IntField(new Rect(min.x + labelWidth + width + spaceWidth, min.y, width - spaceWidth, 18), yProperty.displayName, yProperty.intValue);
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}
#endif