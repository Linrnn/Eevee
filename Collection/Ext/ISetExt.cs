using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class ISetExt
    {
        /// <summary>
        /// foreach ISet`1.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static void Union0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (input == null)
                return;

            switch (input)
            {
                case T[] array:
                    foreach (var item in array)
                        source.Add(item);
                    break;

                case List<T> list:
                    foreach (var item in list)
                        source.Add(item);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        source.Add(item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        source.Add(item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        source.Add(item);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        source.Add(item);
                    break;

                case WeakList<T> weakList:
                    foreach (var item in weakList)
                        source.Add(item);
                    break;

                default: // 存在GC，慎重调用
                    source.UnionWith(input); break;
            }
        }
    }
}