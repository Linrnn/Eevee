#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EeveeEditor
{
    internal readonly struct FindType
    {
        internal static Type GetType(Type basType)
        {
            var types = new List<Type>();
            GetTypes(basType, types);
            if (types.Count != 1)
                Debug.LogWarning($"basType:{basType.FullName}, types.Count:{types.Count} isn't 1");
            if (types.Count == 0)
                return null;
            return types[0];
        }
        internal static void GetTypes(Type basType, ICollection<Type> types)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface)
                        continue;
                    if (type.IsAbstract)
                        continue;
                    if (basType.IsAssignableFrom(type))
                        types.Add(type);
                }
            }
        }
    }
}
#endif