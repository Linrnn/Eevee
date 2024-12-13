using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class ESet
    {
        /// <summary>
        /// IEnumerable`1.GetEnumerator() 的实现导致 ISet`1.UnionWith() 出现GC，所以重写接口
        /// </summary>
        public static void Union_0GC<T>(this ISet<T> source, IEnumerable<T> input)
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

                default: // 存在GC，慎重调用
                    source.UnionWith(input); break;
            }
        }
    }
}