#if UNITY_EDITOR
using Eevee.Collection;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Collection
{
    [CustomPropertyDrawer(typeof(WeakOrderList<>))]
    internal sealed class WeakOrderListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OrderDrawer.OnGUI(position, property, label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            return OrderDrawer.GetPropertyHeight(height, property, label);
        }
    }
}
#endif