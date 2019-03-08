using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace SmartTouch.CRM.Repository
{
    public static class QueryableExtensions
    {
        private static IOrderedQueryable<T> OrderingHelper<T>(IQueryable<T> source, string propertyName, bool descending, bool anotherLevel)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), string.Empty); // I don't care about some naming
            MemberExpression property = Expression.PropertyOrField(param, propertyName);
            LambdaExpression sort = Expression.Lambda(property, param);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(T), property.Type },
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, false, false);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, true, false);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, false, true);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, true, true);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, ListSortDirection direction = ListSortDirection.Descending)
        {
            if (direction == ListSortDirection.Ascending)
                return OrderBy(source, propertyName);
            else
                return OrderByDescending(source, propertyName);
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName, ListSortDirection direction = ListSortDirection.Descending)
        {
            if (direction == ListSortDirection.Ascending)
                return ThenBy(source, propertyName);
            else
                return ThenByDescending(source, propertyName);
        }
        public static void Each<T>(this IQueryable<T> items, Action<T> action)
        {
            if (items == null) return;

            var cached = items;

            foreach (var item in cached)
                action(item);
        }

        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null) return;

            var cached = items;

            foreach (var item in cached)
                action(item);
        }
        public static void Each<T>(this List<T> items, Action<T> action)
        {
            if (items == null) return;

            var cached = items;

            foreach (var item in cached)
                action(item);
        }
    }
}
