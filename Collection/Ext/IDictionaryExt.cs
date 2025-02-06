using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class IDictionaryExt
    {
        /// <summary>
        /// foreach IDictionary`2.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static void Update0GC<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input)
        {
            source.Clear();

            if (input == null)
                return;

            AddRange0GC(source, input);
        }

        /// <summary>
        /// foreach IDictionary`2.GetEnumerator() 会引发GC，故封装 0GC 方法
        /// </summary>
        public static void AddRange0GC<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input)
        {
            if (input == null)
                return;

            switch (input)
            {
                case Dictionary<TKey, TValue> dictionary:
                    foreach (var pair in dictionary)
                        source[pair.Key] = pair.Value;
                    break;

                case SortedDictionary<TKey, TValue> sortedDictionary:
                    foreach (var pair in sortedDictionary)
                        source[pair.Key] = pair.Value;
                    break;

                default: // 存在GC，慎重调用
                    foreach (var pair in input)
                        source[pair.Key] = pair.Value;
                    break;
            }
        }
    }
}