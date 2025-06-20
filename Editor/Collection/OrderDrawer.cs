#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor.Collection
{
    internal readonly struct OrderDrawer
    {
        internal const string Order = "_order";
        internal const string Items = "_items";
        internal const string Size = "_size";

        internal static void OnGUI(in Rect position, SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative(Items);
            var sizeProperty = property.FindPropertyRelative(Size);

            itemsProperty.arraySize = sizeProperty.intValue;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, itemsProperty, label);
            EditorGUI.EndDisabledGroup();

            itemsProperty.Dispose();
            sizeProperty.Dispose();
        }
        internal static float GetPropertyHeight(float height, SerializedProperty property, GUIContent _)
        {
            var itemsProperty = property.FindPropertyRelative(Items);
            if (!itemsProperty.isExpanded)
            {
                itemsProperty.Dispose();
                return height;
            }

            var sizeProperty = property.FindPropertyRelative(Size);

            float elementHeight = height + 2;
            int scale = Math.Max(sizeProperty.intValue, 1);

            sizeProperty.Dispose();
            return height * 3 + elementHeight * scale;
        }
    }
}
#endif