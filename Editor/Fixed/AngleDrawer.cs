#if UNITY_EDITOR
using Eevee.Fixed;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Fixed
{
    [CustomPropertyDrawer(typeof(Angle))]
    internal sealed class AngleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative(nameof(Angle.Value));
            NumberDrawer.OnGUI(in position, valueProperty, label, property.displayName);
        }
    }
}
#endif