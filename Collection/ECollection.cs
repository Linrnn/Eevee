using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class ECollection
    {
        public static bool IsEmpty<T>(this ICollection<T> source)
        {
            return source.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count == 0;
        }

        public static void Update<T>(this ICollection<T> source, IEnumerable<T> input)
        {
            source.Clear();
            AddRange_0GC(source, input);
        }

        /// <summary>
        /// IEnumerable`1.GetEnumerator() 的实现导致 List`1.AddRange() 出现GC，所以重写接口
        /// </summary>
        public static void AddRange_0GC<T>(this ICollection<T> source, IEnumerable<T> input)
        {
            if (input == null)
                return;

            if (input is ICollection<T> collection)
                switch (source)
                {
                    case List<T> list:
                        if (collection.Count + collection.Count > list.Capacity)
                            list.Capacity <<= 1;
                        break;
                }

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
                    foreach (var item in input)
                        source.Add(item);
                    break;
            }
        }

        public static void RemoveRange<T>(this ICollection<T> source, in IEnumerable<T> input)
        {
            switch (input)
            {
                case T[] array:
                    foreach (var item in array)
                        source.Remove(item);
                    break;

                case List<T> list:
                    foreach (var item in list)
                        source.Remove(item);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        source.Remove(item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        source.Remove(item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        source.Remove(item);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        source.Remove(item);
                    break;
            }
        }
    }
}