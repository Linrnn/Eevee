using Eevee.Random;
using System;
using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class IReadOnlyListExt
    {
        private static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> source) => source is null || source.Count == 0;

        public static T RandomGet<T>(this IReadOnlyList<T> source, IRandom random)
        {
            bool empty = source.IsNullOrEmpty();
            int idx = empty ? -1 : random.GetInt32(0, source.Count);
            return empty ? default : source[idx];
        }
        public static bool RandomGet<T>(this IReadOnlyList<T> source, IRandom random, out int index)
        {
            bool empty = source.IsNullOrEmpty();
            index = empty ? -1 : random.GetInt32(0, source.Count);
            return !empty;
        }
        public static bool RandomGet<T>(this IReadOnlyList<T> source, IRandom random, out int index, out T item)
        {
            bool empty = source.IsNullOrEmpty();
            int idx = empty ? -1 : random.GetInt32(0, source.Count);
            index = empty ? -1 : idx;
            item = empty ? default : source[idx];
            return !empty;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> source, T item, IEqualityComparer<T> comparer = null) => IndexOf(source, item, 0, source.Count, comparer);
        public static int IndexOf<T>(this IReadOnlyList<T> source, T item, int index, IEqualityComparer<T> comparer = null) => IndexOf(source, item, index, source.Count - index, comparer);
        public static int IndexOf<T>(this IReadOnlyList<T> source, T item, int index, int count, IEqualityComparer<T> comparer = null)
        {
            switch (source)
            {
                case List<T> list when comparer is null: return list.IndexOf(item, index, count);
                case WeakOrderList<T> weakOrderList when comparer is null: return weakOrderList.IndexOf(item, index, count);
                default:
                    var equalityComparer = comparer ?? EqualityComparer<T>.Default;
                    for (int end = index + count, i = index; i < end; ++i)
                        if (equalityComparer.Equals(item, source[i]))
                            return i;
                    return -1;
            }
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> source, T item, IEqualityComparer<T> comparer = null) => LastIndexOf(source, item, source.Count - 1, source.Count, comparer);
        public static int LastIndexOf<T>(this IReadOnlyList<T> source, T item, int index, IEqualityComparer<T> comparer = null) => LastIndexOf(source, item, index, index + 1, comparer);
        public static int LastIndexOf<T>(this IReadOnlyList<T> source, T item, int index, int count, IEqualityComparer<T> comparer = null)
        {
            if (source.Count == 0)
                return -1;

            switch (source)
            {
                case List<T> list when comparer is null: return list.LastIndexOf(item, index, count);
                case WeakOrderList<T> weakOrderList when comparer is null: return weakOrderList.LastIndexOf(item, index, count);
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

        public static void GetRange<T>(this IReadOnlyList<T> source, int index, int count, ICollection<T> output)
        {
            for (int end = index + count, i = index; i < end; ++i)
                output.Add(source[i]);
        }

        public static T GetMin<T>(this IReadOnlyList<T> source) where T : IComparable<T>
        {
            var value = source[0];
            for (int count = source.Count, i = 1; i < count; ++i)
            {
                var item = source[i];
                if (item.CompareTo(value) < 0)
                    value = item;
            }

            return value;
        }
        public static T GetMax<T>(this IReadOnlyList<T> source) where T : IComparable<T>
        {
            var value = source[0];
            for (int count = source.Count, i = 1; i < count; ++i)
            {
                var item = source[i];
                if (item.CompareTo(value) > 0)
                    value = item;
            }

            return value;
        }
    }
}