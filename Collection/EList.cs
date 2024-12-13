using System.Collections.Generic;

namespace Eevee.Collection
{
    public static class EList
    {
        // todo lrn 未接入 ERandom
        //public static T GetRandomItem<T>(this IList<T> source)
        //{
        //    if (source.IsNullOrEmpty())
        //        return default;

        //    int index = RandomManager.Instance.Range(0, source.Count);
        //    return source[index];
        //}
        //public static int GetRandomIndex<T>(this IList<T> source)
        //{
        //    if (source.IsNullOrEmpty())
        //        return -1;

        //    int index = RandomManager.Instance.Range(0, source.Count);
        //    return index;
        //}
        //public static (int index, T item) GetRandomItemAndIndex<T>(this IList<T> source)
        //{
        //    if (source.IsNullOrEmpty())
        //        return (-1, default);

        //    int index = RandomManager.Instance.Range(0, source.Count);
        //    return (index, source[index]);
        //}

        public static void RemoveLast<T>(this IList<T> source)
        {
            source.RemoveAt(source.Count - 1);
        }
    }
}