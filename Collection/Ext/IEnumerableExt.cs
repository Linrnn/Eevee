using Eevee.Diagnosis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eevee.Collection
{
    public static class IEnumerableExt
    {
        public static bool Has<T>(this IEnumerable<T> source, T item) => source.Contains(item);

        /// <summary>
        /// foreach IEnumerable`1.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static T GetFirst0GC<T>(this IEnumerable<T> source)
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

                case FixedOrderSet<T> fixedOrderSet:
                    foreach (var item in fixedOrderSet)
                        return item;
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in source)
                        return item;
                    break;
            }

            return default;
        }

        /// <summary>
        /// foreach IEnumerable`1.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static T[] ToArray0GC<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case ICollection<T> collection:
                    if (collection.IsNullOrEmpty())
                        return Array.Empty<T>();

                    var output = new T[collection.Count];
                    collection.CopyTo(output, 0);
                    return output;

                case Stack<T> stack: return stack.ToArray();
                case Queue<T> queue: return queue.ToArray();
                default: return source.ToArray(); // 存在GC，慎重调用
            }
        }

        public static string JoinString<T>(this IEnumerable<T> source) => $"[{LogString(source)}]";
        public static string LogString<T>(this IEnumerable<T> source, char separator = ',') => string.Join(separator, source);
    }
}