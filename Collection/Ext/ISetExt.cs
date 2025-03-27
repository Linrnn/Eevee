using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class ISetExt
    {
        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
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

                case WeakOrderList<T> weakOrderList:
                    foreach (var item in weakOrderList)
                        source.Add(item);
                    break;

                case FixedOrderSet<T> fixedOrderSet:
                    foreach (var item in fixedOrderSet)
                        source.Add(item);
                    break;

                default: source.UnionWith(input); break; // 存在GC，慎重调用
            }
        }
    }
}