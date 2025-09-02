#if UNITY_EDITOR
using Eevee.Fixed;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Fixed
{
    [CustomPropertyDrawer(typeof(Vector2DInt16))]
    internal sealed class Vector2DInt16Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var xProperty = property.FindPropertyRelative(nameof(Vector2DInt16.X));
            var yProperty = property.FindPropertyRelative(nameof(Vector2DInt16.Y));
            var oldValue = new Vector2Int(xProperty.intValue, yProperty.intValue);
            var newValue = EditorGUI.Vector2IntField(position, label, oldValue);
            xProperty.intValue = newValue.x;
            yProperty.intValue = newValue.y;
        }
    }
}
#endif