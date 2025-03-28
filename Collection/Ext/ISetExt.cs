using System.Buffers;
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
        public static void UnionWith0GC<T>(this ISet<T> source, IEnumerable<T> input) => source.AddRange0GC(input);
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

            var contains = ArrayPool<T>.Shared.Rent(inputCount);
            int containsCount = 0;

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (readOnlyList[i] is { } item && source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (list[i] is { } item && source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;
            }

            source.Clear();
            for (int i = 0; i < containsCount; ++i)
                source.Add(contains[i]);
            ArrayPool<T>.Shared.Return(contains, true);
        }
        /// <summary>
        /// source ∩ ~input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void ExceptWith0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0)
            {
                return;
            }

            if (ReferenceEquals(source, input))
            {
                source.Clear();
                return;
            }

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int inputCount = readOnlyList.Count, i = 0; i < inputCount; ++i)
                        source.Remove(readOnlyList[i]);
                    break;

                case IList<T> list:
                    for (int inputCount = list.Count, i = 0; i < inputCount; ++i)
                        source.Remove(list[i]);
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

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        source.Remove(item);
                    break;
            }
        }
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

            var contains = ArrayPool<T>.Shared.Rent(inputCount);
            int containsCount = 0;

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (readOnlyList[i] is { } item && source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (list[i] is { } item && source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        if (source.Contains(item))
                            contains[containsCount++] = item;
                    break;
            }

            source.UnionWith0GC(input);
            for (int i = 0; i < containsCount; ++i)
                source.Remove(contains[i]);
            ArrayPool<T>.Shared.Return(contains, true);
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

            GetUniqueAndUnFoundCount(source, input, false, false, ref inputCount, out int uniqueCount);
            return sourceCount == uniqueCount;
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
            if (hasInputCount)
                if (inputCount == 0)
                    return true;
                else if (source.Count < inputCount)
                    return false;

            GetUniqueAndUnFoundCount(source, input, false, true, ref inputCount, out int uniqueCount);
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

            GetUniqueAndUnFoundCount(source, input, false, false, ref inputCount, out int uniqueCount);
            return sourceCount < inputCount && sourceCount == uniqueCount;
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
            if (hasInputCount)
                if (inputCount == 0)
                    return true;
                else if (sourceCount <= inputCount)
                    return false;

            GetUniqueAndUnFoundCount(source, input, false, true, ref inputCount, out int uniqueCount);
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

            GetUniqueAndUnFoundCount(source, input, true, false, ref inputCount, out int uniqueCount);
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

            GetUniqueAndUnFoundCount(source, input, false, true, ref inputCount, out int uniqueCount);
            return sourceCount == inputCount && sourceCount == uniqueCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetUniqueAndUnFoundCount<T>(ISet<T> source, IEnumerable<T> input, bool returnIfUnique, bool returnIfUnFound, ref int inputCount, out int uniqueCount)
        {
            int sourceCount = source.Count;
            if (sourceCount == 0 || ReferenceEquals(source, input))
            {
                uniqueCount = sourceCount;
                return;
            }

            uniqueCount = 0;
            if (inputCount == 0)
            {
                return;
            }

            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int i = 0; i < inputCount; ++i)
                        if (source.Contains(readOnlyList[i]))
                            if (++uniqueCount is { } && returnIfUnique)
                                break;
                            else { }
                        else if (returnIfUnFound)
                            break;
                    break;

                case IList<T> list:
                    for (int i = 0; i < inputCount; ++i)
                        if (source.Contains(list[i]))
                            if (++uniqueCount is { } && returnIfUnique)
                                break;
                            else { }
                        else if (returnIfUnFound)
                            break;
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (source.Contains(item))
                            if (++uniqueCount is { } && returnIfUnique)
                                break;
                            else { }
                        else if (returnIfUnFound)
                            break;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (source.Contains(item))
                            if (++uniqueCount is { } && returnIfUnique)
                                break;
                            else { }
                        else if (returnIfUnFound)
                            break;
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (source.Contains(item))
                            if (++uniqueCount is { } && returnIfUnique)
                                break;
                            else { }
                        else if (returnIfUnFound)
                            break;
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (source.Contains(item))
                            if (++uniqueCount is { } && returnIfUnique)
                                break;
                            else { }
                        else if (returnIfUnFound)
                            break;
                    break;

                default: // 存在GC，慎重调用
                    if (inputCount > 0)
                        foreach (var item in input)
                            if (source.Contains(item))
                                if (++uniqueCount is { } && returnIfUnique)
                                    break;
                                else { }
                            else if (returnIfUnFound)
                                break;
                            else { }
                    else if ((inputCount = 0) is { })
                        foreach (var item in input)
                            if (++inputCount is { } && source.Contains(item))
                                if (++uniqueCount is { } && returnIfUnique)
                                    break;
                                else { }
                            else if (returnIfUnFound)
                                break;
                    break;
            }
        }
    }
}