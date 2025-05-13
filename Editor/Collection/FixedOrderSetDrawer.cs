#if UNITY_EDITOR
using Eevee.Collection;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Collection
{
    [CustomPropertyDrawer(typeof(FixedOrderSet<>))]
    internal sealed class FixedOrderSetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var orderProperty = property.FindPropertyRelative(OrderDrawer.Order);
            OrderDrawer.OnGUI(in position, orderProperty, label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var orderProperty = property.FindPropertyRelative(OrderDrawer.Order);
            float height = base.GetPropertyHeight(orderProperty, label);
            return OrderDrawer.GetPropertyHeight(height, orderProperty, label);
        }
    }
}
#endif