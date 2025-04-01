using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public static class IDictionaryExt
    {
        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void Update0GC<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input, bool allowDuplicate = false)
        {
            source.Clear();
            AddRange0GC(source, input, allowDuplicate);
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void AddRange0GC<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input, bool allowDuplicate = false)
        {
            if (input == null)
                return;

            switch (input)
            {
                case IReadOnlyList<KeyValuePair<TKey, TValue>> readOnlyList:
                    for (int inputCount = readOnlyList.Count, i = 0; i < inputCount; ++i)
                        SetOrAddItem(source, readOnlyList[i], allowDuplicate);
                    break;

                case IList<KeyValuePair<TKey, TValue>> list:
                    for (int inputCount = list.Count, i = 0; i < inputCount; ++i)
                        SetOrAddItem(source, list[i], allowDuplicate);
                    break;

                case Stack<KeyValuePair<TKey, TValue>> stack:
                    foreach (var pair in stack)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                case Queue<KeyValuePair<TKey, TValue>> queue:
                    foreach (var pair in queue)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                case HashSet<KeyValuePair<TKey, TValue>> hashSet:
                    foreach (var pair in hashSet)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                case SortedSet<KeyValuePair<TKey, TValue>> sortedSet:
                    foreach (var pair in sortedSet)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                case Dictionary<TKey, TValue> dictionary:
                    foreach (var pair in dictionary)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                case SortedDictionary<TKey, TValue> sortedDictionary:
                    foreach (var pair in sortedDictionary)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                case FixedOrderDic<TKey, TValue> fixedOrderDic:
                    foreach (var pair in fixedOrderDic)
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;

                default:
                    foreach (var pair in input) // 迭代器可能存在GC
                        SetOrAddItem(source, in pair, allowDuplicate);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetOrAddItem<TKey, TValue>(IDictionary<TKey, TValue> source, in KeyValuePair<TKey, TValue> pair, bool allowDuplicate)
        {
            if (allowDuplicate)
                source[pair.Key] = pair.Value;
            else
                source.Add(pair.Key, pair.Value);
        }
    }
}