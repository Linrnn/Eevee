#if UNITY_EDITOR
using Eevee.Collection;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Collection
{
    [CustomPropertyDrawer(typeof(WeakOrderList<>))]
    internal sealed class WeakOrderListDrawer : PropertyDrawer
    {
        private const string Items = "_items";
        private const string Size = "_size";
        private const string Version = "_version";

        private int _version;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative(Items);
            var sizeProperty = property.FindPropertyRelative(Size);
            int versionValue = (int)property.GetMemberInfo(Version).GetValue();

            if (versionValue == _version)
            {
                sizeProperty.intValue = itemsProperty.arraySize;
            }
            else
            {
                itemsProperty.arraySize = sizeProperty.intValue;
                _version = versionValue;
            }

            EditorGUI.PropertyField(position, itemsProperty, label);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float propertyHeight = base.GetPropertyHeight(property, label);
            var itemsProperty = property.FindPropertyRelative(Items);
            if (!itemsProperty.isExpanded)
                return propertyHeight;

            var sizeProperty = property.FindPropertyRelative(Size);
            int scale = sizeProperty.intValue > 0 ? sizeProperty.intValue + 3 : 4;
            return scale * propertyHeight;
        }
    }
}
#endif