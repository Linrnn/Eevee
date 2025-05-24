using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public static class ICollectionExt
    {
        public static bool IsEmpty(ICollection source) => source.Count == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this ICollection<T> source) => source.Count == 0;

        public static bool IsNullOrEmpty(ICollection source) => source == null || source.Count == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> source) => source == null || source.Count == 0;

        public static void Update<T>(this ICollection<T> source, IList<T> input, int inputIndex, int inputCount)
        {
            source.Clear();
            for (int end = inputIndex + inputCount, i = inputIndex; i < end; ++i)
                source.Add(input[i]);
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void UpdateLowGC<T>(this ICollection<T> source, IEnumerable<T> input)
        {
            source.Clear();
            AddRangeLowGC(source, input);
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void AddRangeLowGC<T>(this ICollection<T> source, IEnumerable<T> input)
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

            Assert.NotReferenceEquals<InvalidOperationException, AssertArgs>(source, input, nameof(input), "source is reference equals input");
            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int inputCount = readOnlyList.Count, i = 0; i < inputCount; ++i)
                        source.Add(readOnlyList[i]);
                    break;

                case IList<T> list:
                    for (int inputCount = list.Count, i = 0; i < inputCount; ++i)
                        source.Add(list[i]);
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

                default:
                    foreach (var item in input) // 迭代器可能存在GC
                        source.Add(item);
                    break;
            }
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void RemoveRangeLowGC<T>(this ICollection<T> source, IEnumerable<T> input)
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

                default:
                    foreach (var item in input) // 迭代器可能存在GC
                        source.Remove(item);
                    break;
            }
        }
    }
}