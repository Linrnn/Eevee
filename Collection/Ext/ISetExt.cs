﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public static class ISetExt
    {
        /// <summary>
        /// source U input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void UnionWithLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (!ReferenceEquals(source, input))
                source.AddRangeLowGC(input);
        }
        /// <summary>
        /// source ∩ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void IntersectWithLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0 || ReferenceEquals(source, input))
                return;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
            {
                source.Clear();
                return;
            }

            if (!hasInputCount)
                IEnumerableExt.TryGetEnumeratedCount<T>(input, out inputCount);

            StackAllocArray<T>.GetSize(inputCount, out int scale, out int capacity);
            var intersects = new StackAllocArray<T>(scale, stackalloc byte[capacity]);
            FillIntersect(source, input, inputCount, ref intersects, out int intersectCount);
            source.Clear();
            for (int offset = 0; offset < intersectCount;)
                source.Add(intersects.Get(ref offset));
            intersects.Dispose();
        }
        /// <summary>
        /// source ∩ ~input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void ExceptWithLowGC<T>(this ISet<T> source, IEnumerable<T> input) => source.RemoveRangeLowGC(input);
        /// <summary>
        /// (source U input) ∩ ~(source ∩ input)<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void SymmetricExceptWithLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0)
            {
                source.UnionWithLowGC(input);
                return;
            }

            if (ReferenceEquals(source, input))
            {
                source.Clear();
                return;
            }

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return;

            if (!hasInputCount)
                IEnumerableExt.TryGetEnumeratedCount<T>(input, out inputCount);

            StackAllocArray<T>.GetSize(inputCount, out int scale, out int capacity);
            var intersects = new StackAllocArray<T>(scale, stackalloc byte[capacity]);
            FillIntersect(source, input, inputCount, ref intersects, out int intersectCount);
            source.UnionWithLowGC(input);
            for (int offset = 0; offset < intersectCount;)
                source.Remove(intersects.Get(ref offset));
            intersects.Dispose();
        }

        /// <summary>
        /// source ⊆ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsSubsetOfLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return true;

            int sourceCount = source.Count;
            if (sourceCount == 0)
                return true;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && sourceCount > inputCount)
                return false;

            CountUniqueAndUnFound(source, input, false, ref inputCount, out int uniqueCount, out int unFoundCount);
            return sourceCount == uniqueCount && unFoundCount >= 0;
        }
        /// <summary>
        /// source ⊇ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsSupersetOfLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return true;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return true;

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (!source.Contains(readOnlyList[i]))
                            return false;
                    return true;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (!source.Contains(list[i]))
                            return false;
                    return true;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (!source.Contains(item))
                            return false;
                    return true;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (!source.Contains(item))
                            return false;
                    return true;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (!source.Contains(item))
                            return false;
                    return true;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (!source.Contains(item))
                            return false;
                    return true;

                default:
                    foreach (var item in input) // 迭代器可能存在GC
                        if (!source.Contains(item))
                            return false;
                    return true;
            }
        }
        /// <summary>
        /// source ⊊ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsProperSubsetOfLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return false;

            int sourceCount = source.Count;
            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && sourceCount >= inputCount)
                return false;

            if (sourceCount == 0)
                return true;

            CountUniqueAndUnFound(source, input, false, ref inputCount, out int uniqueCount, out int unFoundCount);
            return sourceCount == uniqueCount && unFoundCount > 0;
        }
        /// <summary>
        /// source ⊋ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsProperSupersetOfLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return false;

            int sourceCount = source.Count;
            if (sourceCount == 0)
                return false;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return true;

            CountUniqueAndUnFound(source, input, true, ref inputCount, out int uniqueCount, out int unFoundCount);
            return sourceCount > uniqueCount && unFoundCount == 0;
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool OverlapsLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            int sourceCount = source.Count;
            if (sourceCount == 0)
                return false;

            if (ReferenceEquals(source, input))
                return true;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return false;

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (source.Contains(readOnlyList[i]))
                            return true;
                    return false;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (source.Contains(list[i]))
                            return true;
                    return false;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (source.Contains(item))
                            return true;
                    return false;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (source.Contains(item))
                            return true;
                    return false;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (source.Contains(item))
                            return true;
                    return false;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (source.Contains(item))
                            return true;
                    return false;

                default:
                    foreach (var item in input) // 迭代器可能存在GC
                        if (source.Contains(item))
                            return true;
                    return false;
            }
        }
        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool SetEqualsLowGC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return true;

            int sourceCount = source.Count;
            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && sourceCount == 0)
                return inputCount == 0;

            CountUniqueAndUnFound(source, input, true, ref inputCount, out int uniqueCount, out int unFoundCount);
            return sourceCount == uniqueCount && unFoundCount == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillIntersect<T>(ICollection<T> source, IEnumerable<T> input, int inputCount, ref StackAllocArray<T> intersects, out int intersectCount)
        {
            intersectCount = 0;
            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        FillItem(source, readOnlyList[i], ref intersects, ref intersectCount);
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        FillItem(source, list[i], ref intersects, ref intersectCount);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        FillItem(source, item, ref intersects, ref intersectCount);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        FillItem(source, item, ref intersects, ref intersectCount);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        FillItem(source, item, ref intersects, ref intersectCount);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        FillItem(source, item, ref intersects, ref intersectCount);
                    break;

                default:
                    foreach (var item in input) // 迭代器可能存在GC
                        FillItem(source, item, ref intersects, ref intersectCount);
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillItem<T>(ICollection<T> source, T item, ref StackAllocArray<T> intersects, ref int index)
        {
            if (source.Contains(item))
                intersects.Set(ref index, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CountUniqueAndUnFound<T>(ICollection<T> source, IEnumerable<T> input, bool returnIfUnFound, ref int inputCount, out int uniqueCount, out int unFoundCount)
        {
            int sourceCount = source.Count;
            if (sourceCount == 0)
            {
                if (inputCount < 0)
                    IEnumerableExt.TryGetEnumeratedCount<T>(input, out inputCount);
                uniqueCount = 0;
                unFoundCount = inputCount;
                return;
            }

            if (ReferenceEquals(source, input))
            {
                uniqueCount = sourceCount;
                unFoundCount = 0;
                return;
            }

            uniqueCount = 0;
            unFoundCount = 0;
            if (inputCount == 0)
                return;

            int markCount = inputCount;
            StackAllocSet<T>.GetSize(ref markCount, out int scale, out int capacity);
            var mark = new StackAllocSet<T>(scale, stackalloc int[markCount], stackalloc byte[capacity]);
            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, readOnlyList[i], returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, list[i], returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, item, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, item, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, item, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, item, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                default:
                    if (inputCount >= 0)
                        foreach (var item in input) // 迭代器可能存在GC
                            if (CountUniqueAndUnFoundItemIsBreak(source, ref mark, item, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                                break;
                            else { }
                    else if ((inputCount = 0) is { })
                        foreach (var item in input) // 迭代器可能存在GC
                            if (++inputCount is { } && CountUniqueAndUnFoundItemIsBreak(source, ref mark, item, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                                break;
                    break;
            }
            mark.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CountUniqueAndUnFoundItemIsBreak<T>(ICollection<T> source, ref StackAllocSet<T> mark, T item, bool returnIfUnFound, ref int uniqueCount, ref int unFoundCount)
        {
            if (source.Contains(item))
            {
                if (mark.Add(item))
                    ++uniqueCount;
            }
            else
            {
                ++unFoundCount;
                if (returnIfUnFound)
                    return true;
            }

            return false;
        }
    }
}