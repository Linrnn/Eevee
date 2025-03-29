using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eevee.Collection
{
    public static class IDictionaryExt
    {
        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void Update0GC<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input)
        {
            source.Clear();
            if (input == null)
                return;
            AddRange0GC(source, input);
        }

        /// <summary>
        /// 解决“IEnumerable`1.GetEnumerator()”引发的GC
        /// </summary>
        public static void AddRange0GC<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input)
        {
            if (input == null)
                return;

            switch (input)
            {
                case Dictionary<TKey, TValue> dictionary:
                    foreach (var pair in dictionary)
                        SetItem(source, in pair);
                    break;

                case SortedDictionary<TKey, TValue> sortedDictionary:
                    foreach (var pair in sortedDictionary)
                        SetItem(source, in pair);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var pair in input)
                        SetItem(source, in pair);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetItem<TKey, TValue>(IDictionary<TKey, TValue> source, in KeyValuePair<TKey, TValue> pair) => source[pair.Key] = pair.Value;
    }
}