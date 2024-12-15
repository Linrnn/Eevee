using Eevee.Log;
using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class IListExt
    {
        // todo lrn 未接入 ERandom
        //public static T GetRandomItem<T>(this IList<T> source)
        //{
        //    if (source.IsNullOrEmpty())
        //        return default;

        //    int index = RandomManager.Instance.Range(0, source.Count);
        //    return source[index];
        //}
        //public static int GetRandomIndex<T>(this IList<T> source)
        //{
        //    if (source.IsNullOrEmpty())
        //        return -1;

        //    int index = RandomManager.Instance.Range(0, source.Count);
        //    return index;
        //}
        //public static (int index, T item) GetRandomItemAndIndex<T>(this IList<T> source)
        //{
        //    if (source.IsNullOrEmpty())
        //        return (-1, default);

        //    int index = RandomManager.Instance.Range(0, source.Count);
        //    return (index, source[index]);
        //}

        public static int? LastIndexOf<T>(this IList<T> source, T item, IEqualityComparer<T> comparer = null)
        {
            return LastIndexOf(source, item, source.Count - 1, source.Count, comparer);
        }
        public static int? LastIndexOf<T>(this IList<T> source, T item, int index, IEqualityComparer<T> comparer = null)
        {
            return LastIndexOf(source, item, index, index + 1, comparer);
        }
        public static int? LastIndexOf<T>(this IList<T> source, T item, int index, int count, IEqualityComparer<T> comparer = null)
        {
            switch (source)
            {
                case List<T> list when comparer == null:
                    int listIndex = list.LastIndexOf(item, index, count);
                    return listIndex >= 0 ? listIndex : null;

                case WeakList<T> weakList when comparer == null:
                    int weakListIndex = weakList.LastIndexOf(item, index, count);
                    return weakListIndex >= 0 ? weakListIndex : null;

                default:
                    if (source.IsNullOrEmpty())
                        return null;

                    var equalityComparer = comparer ?? EqualityComparer<T>.Default;
                    for (int i = index, end = index - count + 1; i >= end; --i)
                        if (equalityComparer.Equals(item, source[i]))
                            return i;

                    return null;
            }
        }

        public static int? BinarySearch<T>(this IList<T> source, T item, IComparer<T> comparer = null)
        {
            return BinarySearch(source, 0, source.Count, item, comparer);
        }
        public static int? BinarySearch<T>(this IList<T> source, int index, int count, T item, IComparer<T> comparer = null)
        {
            switch (source)
            {
                case List<T> list:
                    int listIndex = list.BinarySearch(index, count, item, comparer);
                    return listIndex >= 0 ? listIndex : null;

                case WeakList<T> weakList:
                    int weakListIndex = weakList.BinarySearch(index, count, item, comparer);
                    return weakListIndex >= 0 ? weakListIndex : null;

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

                    return null;
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

                case WeakList<T> weakList:
                    weakList.Reverse(index, count);
                    return;

                default:
                    int end = index + count;
                    if (end <= source.Count)
                        for (int left = index, right = end - 1; left < right; ++left, --right)
                            (source[left], source[right]) = (source[right], source[left]);
                    else
                        LogRelay.Error($"[Collection] Reverse fail, index + count > length, index:{index}, count:{count}, length:{source.Count}");
                    break;
            }
        }

        /// <summary>
        /// foreach IEnumerable`1.GetEnumerator() 会引发GC，故封装 0GC 方法
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

                    case WeakList<T> weakList:
                        weakList.InsertRange(sourceIndex, input);
                        return;
                }
            }

            int index = sourceIndex;
            switch (input)
            {
                case T[] array:
                    foreach (var item in array)
                        source.Insert(index++, item);
                    break;

                case List<T> list:
                    foreach (var item in list)
                        source.Insert(index++, item);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        source.Insert(index++, item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        source.Insert(index++, item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        source.Insert(index++, item);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        source.Insert(index++, item);
                    break;

                case WeakList<T> weakList:
                    foreach (var item in weakList)
                        source.Insert(index++, item);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        source.Insert(index++, item);
                    break;
            }
        }

        public static void RemoveLast<T>(this IList<T> source)
        {
            source.RemoveAt(source.Count - 1);
        }

        public static void RemoveRange<T>(this IList<T> source, int index, int count)
        {
            int end = index + count;
            if (end > source.Count)
            {
                LogRelay.Error($"[Collection] RemoveRange fail, index + count > end, index:{index}, count:{count}, length:{source.Count}");
                return;
            }

            switch (source)
            {
                case List<T> list: list.RemoveRange(index, count); break;
                case WeakList<T> weakList: weakList.RemoveRange(index, count); break;

                default:
                    for (int i = index, j = end; i < end && j <= count; ++i, ++j)
                        source[i] = source[j];
                    for (int i = 0; i < count; ++i)
                        RemoveLast(source);
                    break;
            }
        }

        public static void Sort<T>(this IList<T> source, IComparer<T> comparer = null) => Sort(source, 0, source.Count, comparer);
        public static void Sort<T>(this IList<T> source, int index, int count, IComparer<T> comparer)
        {
            switch (source)
            {
                case List<T> list: list.Sort(index, count, comparer); break;
                case WeakList<T> weakList: weakList.Sort(index, count, comparer); break;
                default: LogRelay.Error("[Collection] Sort() 未实现"); break;
            }
        }
    }
}