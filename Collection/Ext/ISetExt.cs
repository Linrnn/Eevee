using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

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
            if (source.Count == 0 || ReferenceEquals(source, input) || input is ICollection<T> { Count: 0 })
            {
                return;
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
        /// source ∩ input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void IntersectWith0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0 || ReferenceEquals(source, input))
            {
                return;
            }

            if (input is ICollection<T> { Count: 0 })
            {
                source.Clear();
                return;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            var repeat = ArrayPool<T>.Shared.Rent(input.Count());
            var span = new Span<T>(repeat);

            switch (input)
            {
                case T[] array:
                    foreach (var item in array)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case List<T> list:
                    foreach (var item in list)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case WeakOrderList<T> weakOrderList:
                    foreach (var item in weakOrderList)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case FixedOrderSet<T> fixedOrderSet:
                    foreach (var item in fixedOrderSet)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in input)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;
            }

            source.Clear();
            foreach (var item in span)
                source.Add(item);
            ArrayPool<T>.Shared.Return(repeat, true);
        }
        /// <summary>
        /// source ∩ ~input<br/>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void ExceptWith0GC<T>(this ISet<T> source, IEnumerable<T> input)
        {
            if (source.Count == 0 || input is ICollection<T> { Count: 0 })
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

            if (Equals(source, input))
            {
                source.Clear();
                return;
            }

            if (input is ICollection<T> { Count: 0 })
            {
                return;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            var repeat = ArrayPool<T>.Shared.Rent(input.Count());
            var span = new Span<T>(repeat);

            switch (input)
            {
                case T[] array:
                    foreach (var item in array)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case List<T> list:
                    foreach (var item in list)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case HashSet<T> hashSet:
                    foreach (var item in hashSet)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case SortedSet<T> sortedSet:
                    foreach (var item in sortedSet)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case WeakOrderList<T> weakOrderList:
                    foreach (var item in weakOrderList)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                case FixedOrderSet<T> fixedOrderSet:
                    foreach (var item in fixedOrderSet)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;

                default: // 存在GC，慎重调用
                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var item in input)
                        if (source.Contains(item))
                            span.Fill(item);
                    break;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            source.UnionWith0GC(input);
            foreach (var item in span)
                source.Remove(item);
            ArrayPool<T>.Shared.Return(repeat, true);
        }
    }
}