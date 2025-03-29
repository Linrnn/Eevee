using Eevee.Diagnosis;
using Eevee.Random;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public static class IListExt
    {
        public static T RandomGet<T>(this IList<T> source, IRandom random)
        {
            bool empty = source.IsNullOrEmpty();
            int idx = empty ? -1 : random.GetInt32(0, source.Count);
            return empty ? default : source[idx];
        }
        public static bool RandomGet<T>(this IList<T> source, IRandom random, out int index)
        {
            bool empty = source.IsNullOrEmpty();
            index = empty ? -1 : random.GetInt32(0, source.Count);
            return !empty;
        }
        public static bool RandomGet<T>(this IList<T> source, IRandom random, out int index, out T item)
        {
            bool empty = source.IsNullOrEmpty();
            int idx = empty ? -1 : random.GetInt32(0, source.Count);
            index = empty ? -1 : idx;
            item = empty ? default : source[idx];
            return !empty;
        }

        public static int IndexOf<T>(this IList<T> source, T item, IEqualityComparer<T> comparer = null) => IndexOf(source, item, 0, source.Count, comparer);
        public static int IndexOf<T>(this IList<T> source, T item, int index, IEqualityComparer<T> comparer = null) => IndexOf(source, item, index, source.Count - index, comparer);
        public static int IndexOf<T>(this IList<T> source, T item, int index, int count, IEqualityComparer<T> comparer = null)
        {
            switch (source)
            {
                case List<T> list when comparer == null:
                    int listIndex = list.IndexOf(item, index, count);
                    return listIndex >= 0 ? listIndex : -1;

                case WeakOrderList<T> weakOrderList when comparer == null:
                    int weakListIndex = weakOrderList.IndexOf(item, index, count);
                    return weakListIndex >= 0 ? weakListIndex : -1;

                default:
                    var equalityComparer = comparer ?? EqualityComparer<T>.Default;
                    for (int end = index + count + 1, i = index; i < end; ++i)
                        if (equalityComparer.Equals(item, source[i]))
                            return i;
                    return -1;
            }
        }

        public static int LastIndexOf<T>(this IList<T> source, T item, IEqualityComparer<T> comparer = null) => LastIndexOf(source, item, source.Count - 1, source.Count, comparer);
        public static int LastIndexOf<T>(this IList<T> source, T item, int index, IEqualityComparer<T> comparer = null) => LastIndexOf(source, item, index, index + 1, comparer);
        public static int LastIndexOf<T>(this IList<T> source, T item, int index, int count, IEqualityComparer<T> comparer = null)
        {
            if (source.Count == 0)
                return -1;

            switch (source)
            {
                case List<T> list when comparer == null:
                    int listIndex = list.LastIndexOf(item, index, count);
                    return listIndex >= 0 ? listIndex : -1;

                case WeakOrderList<T> weakOrderList when comparer == null:
                    int weakListIndex = weakOrderList.LastIndexOf(item, index, count);
                    return weakListIndex >= 0 ? weakListIndex : -1;

                default:
                    if (source.IsNullOrEmpty())
                        return -1;
                    var equalityComparer = comparer ?? EqualityComparer<T>.Default;
                    for (int i = index, end = index - count + 1; i >= end; --i)
                        if (equalityComparer.Equals(item, source[i]))
                            return i;
                    return -1;
            }
        }

        public static void GetRange<T>(this IList<T> source, int index, int count, ICollection<T> output)
        {
            for (int end = index + count + 1, i = index; i < end; ++i)
                output.Add(source[i]);
        }

        public static int BinarySearch<T>(this IList<T> source, T item, IComparer<T> comparer = null)
        {
            return BinarySearch(source, 0, source.Count, item, comparer);
        }
        public static int BinarySearch<T>(this IList<T> source, int index, int count, T item, IComparer<T> comparer = null)
        {
            switch (source)
            {
                case List<T> list:
                    int listIndex = list.BinarySearch(index, count, item, comparer);
                    return listIndex >= 0 ? listIndex : -1;

                case WeakOrderList<T> weakOrderList:
                    int weakListIndex = weakOrderList.BinarySearch(index, count, item, comparer);
                    return weakListIndex >= 0 ? weakListIndex : -1;

                default:
                    var finalComparer = comparer ?? Comparer<T>.Default;
                    for (int left = index, right = index + count - 1; left <= right;)
                    {
                        int median = right + left >> 1;
                        int match = finalComparer.Compare(source[median], item);

                        if (match == 0)
                            return median;

                        if (match < 0)
                            left = median + 1;
                        else
                            right = median - 1;
                    }

                    return -1;
            }
        }

        public static void Reverse<T>(this IList<T> source)
        {
            Reverse(source, 0, source.Count);
        }
        public static void Reverse<T>(this IList<T> source, int index, int count)
        {
            switch (source)
            {
                case List<T> list:
                    list.Reverse(index, count);
                    return;

                case WeakOrderList<T> weakOrderList:
                    weakOrderList.Reverse(index, count);
                    return;

                default:
                    for (int end = index + count, left = index, right = end - 1; left < right; ++left, --right)
                        (source[left], source[right]) = (source[right], source[left]);
                    break;
            }
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void InsertRange0GC<T>(this IList<T> source, int sourceIndex, IEnumerable<T> input)
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
                        list.InsertRange(sourceIndex, input);
                        return;

                    case WeakOrderList<T> weakOrderList:
                        weakOrderList.InsertRange(sourceIndex, input);
                        return;
                }
            }

            int sourceCount = sourceIndex;
            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int inputCount = readOnlyList.Count, i = 0; i < inputCount; ++i)
                        InsertItem(source, readOnlyList[i], ref sourceCount);
                    break;

                case IList<T> list:
                    for (int inputCount = list.Count, i = 0; i < inputCount; ++i)
                        InsertItem(source, list[i], ref sourceCount);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        InsertItem(source, item, ref sourceCount);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        InsertItem(source, item, ref sourceCount);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        InsertItem(source, item, ref sourceCount);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        InsertItem(source, item, ref sourceCount);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        InsertItem(source, item, ref sourceCount);
                    break;
            }
        }

        public static void RemoveLast<T>(this IList<T> source)
        {
            source.RemoveAt(source.Count - 1);
        }

        public static void RemoveRange<T>(this IList<T> source, int index, int count)
        {
            switch (source)
            {
                case List<T> list: list.RemoveRange(index, count); break;
                case WeakOrderList<T> weakOrderList: weakOrderList.RemoveRange(index, count); break;
                default:
                    for (int end = index + count, i = index, j = end; i < end && j <= count; ++i, ++j)
                        source[i] = source[j];
                    for (int i = 0; i < count; ++i)
                        RemoveLast(source);
                    break;
            }
        }

        public static void SetAll<T>(this IList<T> source, T item)
        {
            for (int i = 0; i < source.Count; ++i)
                source[i] = item;
        }
        public static void SetAll<T>(this IList<T> source, T item, int index, int count)
        {
            for (int end = index + count + 1, i = index; i < end; ++i)
                source[i] = item;
        }

        public static void Sort<T>(this IList<T> source, IComparer<T> comparer = null) => Sort(source, 0, source.Count, comparer);
        public static void Sort<T>(this IList<T> source, int index, int count, IComparer<T> comparer)
        {
            switch (source)
            {
                case List<T> list: list.Sort(index, count, comparer); break;
                case WeakOrderList<T> weakOrderList: weakOrderList.Sort(index, count, comparer); break;
                default: LogRelay.Error("[Collection] Sort() 未实现"); break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertItem<T>(IList<T> source, T item, ref int length) => source.Insert(length++, item);
    }
}