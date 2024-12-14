using Eevee.Log;
using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class IEnumerableExt
    {
        public static T GetFirst<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case T[] array:
                    foreach (var item in array)
                        return item;
                    break;

                case List<T> list:
                    foreach (var item in list)
                        return item;
                    break;

                case Stack<T> stack:
                    foreach (var item in stack)
                        return item;
                    break;

                case Queue<T> queue:
                    foreach (var item in queue)
                        return item;
                    break;

                case HashSet<T> hashSet:
                    LogRelay.Error("[Collection] HashSet<T>无序，GetFirst()无法保证结果的一致性");
                    foreach (var item in hashSet)
                        return item;
                    break;

                default: // 存在GC，慎重调用
                    foreach (var item in source)
                        return item;

                    break;
            }

            return default;
        }

        public static List<T> DeepCopy2List<T>(this IEnumerable<T> source)
        {
            var collection = source as ICollection<T>;
            // todo lrn 未接入 EPool
            var output = collection == null ? new List<T>() : new List<T>(collection.Count);
            output.AddRange_0GC(source);
            return output;
        }
    }
}