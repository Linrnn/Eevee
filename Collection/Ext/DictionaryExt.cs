using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class DictionaryExt
    {
        public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            return source.Count == 0;
        }

        public static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            return source == null || source.Count == 0;
        }

        public static void UpdateDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input)
        {
            source.Clear();
            if (input == null)
                return;

            switch (input)
            {
                case Dictionary<TKey, TValue> dictionary:
                    foreach (var pair in dictionary)
                        source.Add(pair.Key, pair.Value);
                    break;

                default: // 存在GC，慎重调用
                    foreach (var pair in input)
                        source.Add(pair.Key, pair.Value);
                    break;
            }
        }

        public static void AddRangeDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> input)
        {
            if (input == null)
                return;

            switch (input)
            {
                case Dictionary<TKey, TValue> dictionary:
                    foreach (var pair in dictionary)
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