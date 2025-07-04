﻿using Eevee.Diagnosis;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eevee.Collection
{
    public static class IEnumerableExt
    {
        internal static bool TryGetNonEnumeratedCount<T>(object source, out int count)
        {
            switch (source)
            {
                case IReadOnlyCollection<T> readOnlyCollection:
                    count = readOnlyCollection.Count;
                    return true;

                case ICollection<T> collection:
                    count = collection.Count;
                    return true;

                case ICollection collection:
                    count = collection.Count;
                    return true;

                default:
                    count = -1;
                    return false;
            }
        }
        internal static bool TryGetEnumeratedCount<T>(object source, out int count)
        {
            switch (source)
            {
                case IReadOnlyCollection<T> readOnlyCollection:
                    count = readOnlyCollection.Count;
                    return true;

                case ICollection<T> collection:
                    count = collection.Count;
                    return true;

                case ICollection collection:
                    count = collection.Count;
                    return true;

                case IEnumerable<T> enumerable:
                    count = enumerable.Count(); // 迭代器可能存在GC
                    return true;

                case IEnumerable enumerable:
                    count = 0;
                    foreach (object _ in enumerable) // 迭代器可能存在GC
                        ++count;
                    return true;

                default:
                    count = -1;
                    return false;
            }
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static T GetFirstLowGC<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case IReadOnlyList<T> readOnlyList: return readOnlyList[0];
                case IList<T> list: return list[0];

                case Stack<T> stack:
                    foreach (var item in stack)
                        return item;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        return item;
                    break;

                case HashSet<T> hashSet:
                    LogRelay.Error("[Collection] HashSet<T>无序，GetFirst()无法保证结果的一致性");
                    foreach (var item in hashSet)
                        return item;
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        return item;
                    break;

                default:
                    foreach (var item in source) // 迭代器可能存在GC
                        return item;
                    break;
            }

            return default;
        }

        public static string JoinString<T>(this IEnumerable<T> source) => $"[{LogString(source)}]";
        public static string LogString<T>(this IEnumerable<T> source, char separator = ',') => string.Join(separator, source);
    }
}