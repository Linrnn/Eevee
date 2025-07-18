#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace EeveeEditor
{
    internal struct PropertyHandle
    {
        private SerializedObject _serializedObject;
        private Dictionary<string, SerializedProperty> _serializedProperties;

        internal void Initialize(Editor handle)
        {
            _serializedObject = handle.serializedObject;
            _serializedProperties ??= new Dictionary<string, SerializedProperty>();
        }
        internal void Dispose()
        {
            foreach (var (_, property) in _serializedProperties)
                property.Dispose();
            _serializedProperties.Clear();
            _serializedObject.Dispose();
        }

        internal SerializedProperty Get(string path)
        {
            if (_serializedProperties.TryGetValue(path, out var oldProperty))
                return oldProperty;
            var newProperty = _serializedObject.FindProperty(path);
            _serializedProperties.Add(path, newProperty);
            return newProperty;
        }
        internal PropertyHandle Draw(string path, bool allowSet = true)
        {
            var property = Get(path);
            if (allowSet)
            {
                EditorGUILayout.PropertyField(property);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property);
                EditorGUI.EndDisabledGroup();
            }
            return this;
        }
    }
}
#endif