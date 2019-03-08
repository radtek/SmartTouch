using System;
using System.Collections.Generic;

namespace LandmarkIT.Enterprise.CommunicationManager.Extensions
{
    public static class EnumerableExtensions
    {
        public static Dictionary<TKey, TSource> ToSafeDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToSafeDictionary(keySelector, (value) => value, EqualityComparer<TKey>.Default);
        }

        public static Dictionary<TKey, TSource> ToSafeDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToSafeDictionary(keySelector, (value) => value, comparer);
        }

        public static Dictionary<TKey, TElement> ToSafeDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.ToSafeDictionary(keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        public static Dictionary<TKey, TElement> ToSafeDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            var dictionary = new Dictionary<TKey, TElement>(comparer);

            foreach (var item in source)
            {
                var key = keySelector(item);

                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, elementSelector(item));
                }
            }

            return dictionary;
        }
    }
}
