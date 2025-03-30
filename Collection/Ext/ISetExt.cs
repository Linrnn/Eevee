using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public static class ISetExt
    {
        /// <summary>
        /// source U input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void UnionWith0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (!ReferenceEquals(source, input))
                source.AddRange0GC(input);
        }
        /// <summary>
        /// source ∩ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void IntersectWith0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0 || ReferenceEquals(source, input))
            {
                return;
            }

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
            {
                source.Clear();
                return;
            }

            if (!hasInputCount)
            {
                IEnumerableExt.TryGetEnumeratedCount<T>(input, out inputCount);
            }

            var intersects = ArrayExt.SharedRent<T>(inputCount);
            var intersectSpan = intersects.AsSpan(0, inputCount);

            FillIntersect(source, input, inputCount, in intersectSpan, out int intersectCount);
            source.Clear();
            foreach (var item in intersectSpan[..intersectCount])
                source.Add(item);

            intersects.SharedReturn();
        }
        /// <summary>
        /// source ∩ ~input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void ExceptWith0GC<T>(this ISet<T> source, IEnumerable<T> input) => source.RemoveRange0GC(input);
        /// <summary>
        /// (source U input) ∩ ~(source ∩ input)<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void SymmetricExceptWith0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0)
            {
                source.UnionWith0GC(input);
                return;
            }

            if (ReferenceEquals(source, input))
            {
                source.Clear();
                return;
            }

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
            {
                return;
            }

            if (!hasInputCount)
            {
                IEnumerableExt.TryGetEnumeratedCount<T>(input, out inputCount);
            }

            var intersects = ArrayExt.SharedRent<T>(inputCount);
            var intersectSpan = intersects.AsSpan(0, inputCount);

            FillIntersect(source, input, inputCount, in intersectSpan, out int intersectCount);
            source.UnionWith0GC(input);
            foreach (var item in intersectSpan[..intersectCount])
                source.Remove(item);

            intersects.SharedReturn();
        }

        /// <summary>
        /// source ⊆ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsSubsetOf0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return true;

            int sourceCount = source.Count;
            if (sourceCount == 0)
                return true;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && sourceCount > inputCount)
                return false;

            CountUniqueCount(source, input, false, false, ref inputCount, out int uniqueCount, out int _);
            return sourceCount <= inputCount && sourceCount <= uniqueCount;
        }
        /// <summary>
        /// source ⊇ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsSupersetOf0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return true;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return true;

            CountUniqueCount(source, input, false, true, ref inputCount, out int uniqueCount, out int _);
            return inputCount == uniqueCount;
        }
        /// <summary>
        /// source ⊊ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsProperSubsetOf0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return false;

            int sourceCount = source.Count;
            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && sourceCount >= inputCount)
                return false;

            if (sourceCount == 0)
                return true;

            CountUniqueCount(source, input, false, false, ref inputCount, out int uniqueCount, out int _);
            return sourceCount < inputCount && sourceCount < uniqueCount;
        }
        /// <summary>
        /// source ⊋ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool IsProperSupersetOf0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return false;

            int sourceCount = source.Count;
            if (sourceCount == 0)
                return false;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return true;

            CountUniqueCount(source, input, false, true, ref inputCount, out int uniqueCount, out int _);
            return sourceCount > inputCount && inputCount == uniqueCount;
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool Overlaps0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            int sourceCount = source.Count;
            if (sourceCount == 0)
                return false;

            if (ReferenceEquals(source, input))
                return true;

            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount && inputCount == 0)
                return false;

            CountUniqueCount(source, input, false, true, ref inputCount, out int uniqueCount, out int _);
            return uniqueCount > 0;
        }
        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static bool SetEquals0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (ReferenceEquals(source, input))
                return true;

            int sourceCount = source.Count;
            bool hasInputCount = IEnumerableExt.TryGetNonEnumeratedCount<T>(input, out int inputCount);
            if (hasInputCount)
                if (sourceCount != inputCount)
                    return false;
                else if (sourceCount == 0)
                    return true;

            CountUniqueCount(source, input, false, true, ref inputCount, out int uniqueCount, out int _);
            return sourceCount == inputCount && sourceCount == uniqueCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillIntersect<T>(ICollection<T> source, IEnumerable<T> input, int inputCount, in Span<T> intersects, out int intersectCount)
        {
            intersectCount = 0;
            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        FillItem(source, readOnlyList[i], in intersects, ref intersectCount);
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        FillItem(source, list[i], in intersects, ref intersectCount);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        FillItem(source, item, in intersects, ref intersectCount);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        FillItem(source, item, in intersects, ref intersectCount);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        FillItem(source, item, in intersects, ref intersectCount);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        FillItem(source, item, in intersects, ref intersectCount);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        FillItem(source, item, in intersects, ref intersectCount);
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillItem<T>(ICollection<T> source, T item, in Span<T> array, ref int index)
        {
            if (source.Contains(item))
                array[index++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CountUniqueCount<T>(ICollection<T> source, IEnumerable<T> input, bool returnIfUnique, bool returnIfUnFound, ref int inputCount, out int uniqueCount, out int unFoundCount)
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
            {
                return;
            }

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (CountUniqueCountItemIsBreak(source, readOnlyList[i], returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (CountUniqueCountItemIsBreak(source, list[i], returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (CountUniqueCountItemIsBreak(source, item, returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (CountUniqueCountItemIsBreak(source, item, returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (CountUniqueCountItemIsBreak(source, item, returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (CountUniqueCountItemIsBreak(source, item, returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                            break;
                    break;

                default: // 存在GC，慎重调用
                    if (inputCount >= 0)
                        foreach (var item in input)
                            if (CountUniqueCountItemIsBreak(source, item, returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                                break;
                            else { }
                    else if ((inputCount = 0) is { })
                        foreach (var item in input)
                            if (++inputCount is { } && CountUniqueCountItemIsBreak(source, item, returnIfUnique, returnIfUnFound, ref uniqueCount, ref unFoundCount))
                                break;
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CountUniqueCountItemIsBreak<T>(ICollection<T> source, T item, bool returnIfUnique, bool returnIfUnFound, ref int uniqueCount, ref int unFoundCount)
        {
            if (source.Contains(item))
            {
                ++uniqueCount;
                if (returnIfUnique)
                {
                    return true;
                }
            }
            else if (returnIfUnFound)
            {
                ++unFoundCount;
                return true;
            }

            return false;
        }
    }
}