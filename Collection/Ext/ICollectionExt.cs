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

        public static bool IsNullOrEmpty(ICollection source) => source is not { Count: not 0 };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> source) => source is not { Count: not 0 };

        public static void CleanAll<T>(this ICollection<T> source) => source.Clear();

        public static void Update<T>(this ICollection<T> source, IList<T> input, int inputIndex, int inputCount)
        {
            source.Clear();
            for (int end = inputIndex + inputCount, i = inputIndex; i < end; ++i)
                source.Add(input[i]);
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void Update0GC<T>(this ICollection<T> source, IEnumerable<T> input)
        {
            source.Clear();
            AddRange0GC(source, input);
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
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

            Assert.NotReferenceEquals<InvalidOperationException, AssertArgs>(source, input, nameof(input), "source is reference equals input");
            switch (input)
            {
                case IReadOnlyList<T> readOnlyList:
                    for (int inputCount = readOnlyList.Count, i = 0; i < inputCount; ++i)
                        AddItem(source, readOnlyList[i]);
                    break;

                case IList<T> list:
                    for (int inputCount = list.Count, i = 0; i < inputCount; ++i)
                        AddItem(source, list[i]);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        AddItem(source, item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        AddItem(source, item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        AddItem(source, item);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        AddItem(source, item);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        AddItem(source, item);
                    break;
            }
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void RemoveRange0GC<T>(this ICollection<T> source, IEnumerable<T> input)
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
                        RemoveItem(source, readOnlyList[i]);
                    break;

                case IList<T> list:
                    for (int inputCount = list.Count, i = 0; i < inputCount; ++i)
                        RemoveItem(source, list[i]);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        RemoveItem(source, item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        RemoveItem(source, item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        RemoveItem(source, item);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        RemoveItem(source, item);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        RemoveItem(source, item);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddItem<T>(ICollection<T> source, T item) => source.Add(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RemoveItem<T>(ICollection<T> source, T item) => source.Remove(item);
    }
}