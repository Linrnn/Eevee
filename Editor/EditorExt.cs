#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

namespace EeveeEditor.Collection
{
    internal static class EditorExt
    {
        internal readonly struct MemberAbout
        {
            private readonly MemberInfo _info;
            private readonly object _instance;

            internal MemberAbout(MemberInfo info, object instance)
            {
                _info = info;
                _instance = instance;
            }
            internal object GetValue() => _info.GetValue(_instance);
            internal void SetValue(object value) => _info.SetValue(_instance, value);
        }

        internal static MemberAbout GetMemberInfo(this SerializedProperty property, string name)
        {
            var unityObject = property.serializedObject.targetObject;
            var propertyMemberInfo = GetMemberInfo(unityObject.GetType(), property.propertyPath);
            object propertyInstance = propertyMemberInfo.GetValue(unityObject);
            var nameMemberInfo = GetMemberInfo(propertyInstance.GetType(), name);
            return new MemberAbout(nameMemberInfo, propertyInstance);
        }
        internal static MemberInfo GetMemberInfo(this Type type, string name)
        {
            var fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo != null)
                return fieldInfo;

            var propertyInfo = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty);
            if (propertyInfo != null)
                return propertyInfo;

            return null;
        }

        internal static object GetValue(this MemberInfo memberInfo, object obj) => memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
            _ => new ArgumentOutOfRangeException($"[Editor] {memberInfo?.MemberType} 异常"),
        };
        internal static void SetValue(this MemberInfo memberInfo, object obj, object value)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo: fieldInfo.SetValue(obj, value); break;
                case PropertyInfo propertyInfo: propertyInfo.SetValue(obj, value); break;
                default: throw new ArgumentOutOfRangeException($"[Editor] {memberInfo.MemberType} 异常");
            }
        }
    }
}
#endif