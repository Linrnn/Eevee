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
        private const string X = "X";
        private const string Y = "Y";
        private static readonly GUIContent _xLabel = new(X);
        private static readonly GUIContent _yLabel = new(Y);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const int scale = 2;
            const float spaceWidth = EditorUtils.SpaceWidth / scale;
            const float propertyHeight = EditorUtils.PropertyHeight;

            var min = position.position;
            float labelWidth = EditorGUIUtility.labelWidth;
            float width = (position.size.x - labelWidth) / scale;
            var propertyPosition = new Rect(min, new Vector2(labelWidth, propertyHeight));
            var xPosition = new Rect(min.x + labelWidth + spaceWidth, min.y, width - spaceWidth, propertyHeight);
            var yPosition = new Rect(min.x + labelWidth + width + spaceWidth, min.y, width - spaceWidth, propertyHeight);

            var xProperty = property.FindPropertyRelative(X);
            var yProperty = property.FindPropertyRelative(Y);

            EditorGUI.LabelField(propertyPosition, label);
            EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(_xLabel) + spaceWidth;
            xProperty.intValue = EditorGUI.IntField(xPosition, xProperty.displayName, xProperty.intValue);
            EditorGUIUtility.labelWidth = EditorUtils.GetLabelWidth(_yLabel) + spaceWidth;
            yProperty.intValue = EditorGUI.IntField(yPosition, yProperty.displayName, yProperty.intValue);
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}
#endif