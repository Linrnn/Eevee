#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace EeveeEditor
{
    internal readonly struct EnumHandle
    {
        private static readonly Dictionary<Type, (int[] enumValues, string[] enumNames)> _enums = new();

        internal static int[] GetEnumValues(Type enumType) => enumType.IsEnum ? Get(enumType).enumValues : null;
        internal static string[] GetEnumNames(Type enumType) => enumType.IsEnum ? Get(enumType).enumNames : null;

        private static (int[] enumValues, string[] enumNames) Get(Type enumType)
        {
            if (_enums.TryGetValue(enumType, out var enumHandle))
                return enumHandle;

            var enumValues = enumType.GetEnumValues();
            int[] intEnumValues = new int[enumValues.Length];
            for (int i = 0; i < enumValues.Length; ++i)
                intEnumValues[i] = Convert.ToInt32(enumValues.GetValue(i));

            var newEnumHandle = (intEnumValues, enumType.GetEnumNames());
            _enums.Add(enumType, newEnumHandle);
            return newEnumHandle;
        }
    }
}
#endif