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

            orderProperty.Dispose();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var orderProperty = property.FindPropertyRelative(OrderDrawer.Order);

            float baseHeight = base.GetPropertyHeight(orderProperty, label);
            float height = OrderDrawer.GetPropertyHeight(baseHeight, orderProperty, label);

            orderProperty.Dispose();
            return height;
        }
    }
}
#endif