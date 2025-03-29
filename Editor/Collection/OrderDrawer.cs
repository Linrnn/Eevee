#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Collection
{
    internal readonly struct OrderDrawer
    {
        internal const string Order = "_order";
        internal const string Items = "_items";
        internal const string Size = "_size";

        internal static void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative(Items);
            var sizeProperty = property.FindPropertyRelative(Size);

            itemsProperty.arraySize = sizeProperty.intValue;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, itemsProperty, label);
            EditorGUI.EndDisabledGroup();
        }
        internal static float GetPropertyHeight(float height, SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative(Items);
            if (!itemsProperty.isExpanded)
                return height;

            var sizeProperty = property.FindPropertyRelative(Size);
            int scale = sizeProperty.intValue + 4;
            return height * scale;
        }
    }
}
#endif