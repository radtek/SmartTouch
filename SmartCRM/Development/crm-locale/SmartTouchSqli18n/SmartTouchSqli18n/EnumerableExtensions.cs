using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouchSqli18n
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 
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
            return a.Where(aItem =>
                {
                    return !b.Any(bItem => (selectKeyB(bItem) as string) == (selectKeyA(aItem) as string));
                });
        }
    }
}
