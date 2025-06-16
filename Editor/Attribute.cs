#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EeveeEditor
{
    #region SingleEnum
    internal sealed class SingleEnumAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(SingleEnumAttribute))]
    internal sealed class SingleEnumPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var uObject = property.serializedObject.targetObject;
            var objectType = uObject.GetType();
            var field = objectType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic);
            object enumValue = field?.GetValue(uObject);

            var enumPopup = EditorGUI.EnumPopup(position, label, (Enum)enumValue);
            property.enumValueFlag = Convert.ToInt32(enumPopup);
        }
    }
    #endregion

    #region ReadOnly
    /// <summary>
    /// 适用于单高度字段（即GetPropertyHeight()为初始值）
    /// </summary>
    internal sealed class ReadOnlyAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }
    #endregion
}
#endif