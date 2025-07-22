#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace EeveeEditor
{
    internal struct PropertyHandle
    {
        private SerializedObject _serializedObject;
        private Dictionary<string, SerializedProperty> _properties;
        private Dictionary<string, ReorderableList> _reorderableLists;

        internal void Initialize(Editor handle)
        {
            _serializedObject = handle.serializedObject;
            _properties ??= new Dictionary<string, SerializedProperty>();
            _reorderableLists ??= new Dictionary<string, ReorderableList>();
        }
        internal void Dispose()
        {
            foreach (var (_, property) in _properties)
                property.Dispose();
            _properties.Clear();
            _serializedObject.Dispose();
            _reorderableLists.Clear();
        }

        internal SerializedProperty Get(string path)
        {
            if (_properties.TryGetValue(path, out var oldProperty))
                return oldProperty;
            var newProperty = _serializedObject.FindProperty(path);
            _properties.Add(path, newProperty);
            return newProperty;
        }

        internal PropertyHandle Draw(string path, bool disabled = false)
        {
            var property = Get(path);
            EditorGUI.BeginDisabledGroup(disabled);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();
            return this;
        }
        internal PropertyHandle DrawScript()
        {
            var property = Get(EditorUtils.Script);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();
            return this;
        }
        internal PropertyHandle DrawEnum(string path, Type enumType, bool disabled = false)
        {
            var property = Get(path);
            EditorGUI.BeginDisabledGroup(disabled);

            if (property.isArray)
            {
                if (_reorderableLists.TryGetValue(path, out var reorderableList))
                {
                    reorderableList.DoLayoutList();
                }
                else if (enumType is not null)
                {
                    string[] enumNames = enumType.GetEnumNames();
                    _reorderableLists.Add(path, new ReorderableList(_serializedObject, property)
                    {
                        drawHeaderCallback = rect => EditorGUI.LabelField(rect, property.displayName),
                        drawElementCallback = (rect, index, _, _) =>
                        {
                            var element = property.GetArrayElementAtIndex(index);
                            element.intValue = EditorGUI.Popup(rect, element.displayName, element.intValue, enumNames);
                        },
                    });
                }
            }
            else
            {
                property.intValue = EditorGUILayout.Popup(property.displayName, property.intValue, enumType.GetEnumNames());
            }

            EditorGUI.EndDisabledGroup();
            return this;
        }
    }
}
#endif