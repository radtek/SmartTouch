using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace LandmarkIT.Enterprise.Extensions
{

    public static class EnumerableExtentions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, params T[] items)
        {
            return source.Concat(items);
        }
        /// <summary>
        /// Returns the maximal element of the given sequence, based on
        /// the given projection.
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Returns the maximal element of the given sequence, based on
        /// the given projection and the specified comparer for projected values. 
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparer">Comparer to use to compare projected values</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
        /// or <paramref name="comparer"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            if (comparer == null) throw new ArgumentNullException("comparer");
            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }

        /// <summary>
        /// Loops through each item in the IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="action"></param>
        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                if (action != null)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Compare two lists on properties
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="selectKeyA"></param>
        /// <param name="selectKeyB"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<TA> Except<TA, TB, TK>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TK> selectKeyA, Func<TB, TK> selectKeyB, IEqualityComparer<TK> comparer = null)
        {
            return a.Where(aItem => !b.Select(bItem => selectKeyB(bItem)).Contains(selectKeyA(aItem), comparer));
        }

        /// <summary>
        /// Check if null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool IsAny<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static string HtmlSanitizer(this string html)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowDataAttributes = true;
            sanitizer.KeepChildNodes = true;
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("id");
            sanitizer.AllowedCssProperties.Add("word-break");
            var parsedCOntent = sanitizer.HtmlParserFactory().Parse(html);
            var sanitizedHmtl = sanitizer.Sanitize(parsedCOntent.Body.OuterHtml, "");
            return sanitizedHmtl;
        }

        public static string CharactersToHtmlCodes(this string html)
        {
            StringBuilder result = new StringBuilder(html.Length + (int)(html.Length * 0.1));
            foreach (char c in html)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);

            }

            return result.ToString();
        }

        public static string GetTable<T>(this IEnumerable<T> list, params Expression<Func<T, object>>[] fxns)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<table cellpadding='0' cellspacing='0' border='0' width='100%' style='border:solid 1px #d9d9d9;'>");
            sb.Append(Environment.NewLine);
            sb.Append("<tr >");
            foreach (var field in fxns)
            {
                var prop = (PropertyInfo)(field.Body as MemberExpression ?? ((UnaryExpression)field.Body).Operand as MemberExpression).Member;
                var name = prop.GetCustomAttribute<DisplayNameAttribute>().DisplayName.Replace(" ","&nbsp;");
                sb.Append("<td style='margin:0px;padding:10px;font-size:12px; font-weight:bold; background-color:#f5f5f5; color:#555555;font-family:arial,sans-serif; border-bottom:solid 1px #d9d9d9;'>");
                sb.Append(name);
                sb.Append("</td>");
                sb.Append(Environment.NewLine);
            }

            sb.Append("</tr>");
            sb.Append(Environment.NewLine);
            foreach (var item in list)
            {
                sb.Append("<tr>");
                foreach (var fxn in fxns)
                {
                    var fn = fxn.Compile();
                    sb.Append("<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>");
                    sb.Append(fn(item));
                    sb.Append("</td>");
                    sb.Append(Environment.NewLine);
                }
                sb.Append("</tr>");
                sb.Append(Environment.NewLine);
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        public static IEnumerable<int> ToIntList(this string str)
        {
            if (string.IsNullOrEmpty(str)) yield break;

            foreach (var s in str.Split('|'))
            {
                var num = default(int);
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }

        public static DataTable ToContactIdTypeDataTable(this List<int> contactIds)
        {
            var table = new DataTable();
            table.Columns.Add("ContactID", typeof(int));
            foreach (var array in contactIds)
            {
                table.Rows.Add(array);
            }
            return table;
        }
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        public static U GetOrDefault<T, U>(this Dictionary<T, U> dic, T key)
        {
            if (dic.ContainsKey(key)) return dic[key];
            return default(U);
        }
    }
}
