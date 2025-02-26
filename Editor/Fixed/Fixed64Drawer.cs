﻿#if UNITY_EDITOR
using Eevee.Fixed;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 此脚本需要“Fixed64”不是“readonly”才能生效
/// </summary>
[CustomPropertyDrawer(typeof(Fixed64))]
internal sealed class Fixed64Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float width = position.width / 3F;
        var rawValuePosition = new Rect(position.x, position.y, width * 2F, position.height);
        var displayPosition = new Rect(position.x + width * 2F, position.y, width, position.height);

        var rawValueProperty = property.FindPropertyRelative(nameof(Fixed64.RawValue));
        rawValueProperty.longValue = EditorGUI.LongField(rawValuePosition, property.displayName, rawValueProperty.longValue);
        //rawValueProperty.longValue = ((Fixed64)EditorGUI.DoubleField(displayPosition, (double)(new Fixed64(rawValueProperty.longValue)))).RawValue;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.TextField(displayPosition, (new Fixed64(rawValueProperty.longValue)).ToString());
        EditorGUI.EndDisabledGroup();
    }
}
#endif