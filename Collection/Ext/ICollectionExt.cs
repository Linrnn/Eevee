using Eevee.Diagnosis;
using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class ICollectionExt
    {
        public static bool IsEmpty<T>(this ICollection<T> source) => source.Count == 0;

        public static bool IsNullOrEmpty<T>(this ICollection<T> source) => source == null || source.Count == 0;

        public static void Clean<T>(this ICollection<T> source) => source.Clear();

        public static void Update<T>(this ICollection<T> source, IList<T> input, int inputIndex, int inputCount)
        {
            int end = inputIndex + inputCount;
            Assert.IsLessEqual(end, input.Count, "Update fail, index + count > end, index:{0}, count:{1}, length:{2}", inputIndex, inputCount, input.Count);

            source.Clear();
            for (int i = inputIndex; i < end; ++i)
                source.Add(input[i]);
        }

        /// <summary>
        /// foreach ICollection`1.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static void Update0GC<T>(this ICollection<T> source, IEnumerable<T> input)
        {
            source.Clear();
            AddRange0GC(source, input);
        }

        /// <summary>
        /// foreach ICollection`1.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static void AddRange0GC<T>(this ICollection<T> source, IEnumerable<T> input)
        {
            if (input == null)
            {
                return;
            }

            if (input is ICollection<T>)
            {
                switch (source)
                {
                    case List<T> list:
                        list.AddRange(input);
                        return;

                    case WeakOrderList<T> weakOrderList:
                        weakOrderList.InsertRange(source.Count, input);
                        return;
                }
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

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        source.Add(item);
                    break;
            }
        }

        /// <summary>
        /// foreach ICollection`1.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static void RemoveRange0GC<T>(this ICollection<T> source, in IEnumerable<T> input)
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

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        source.Remove(item);
                    break;

                case WeakOrderList<T> weakOrderList:
                    foreach (var item in weakOrderList)
                        source.Remove(item);
                    break;

                case FixedOrderSet<T> fixedOrderSet:
                    foreach (var item in fixedOrderSet)
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