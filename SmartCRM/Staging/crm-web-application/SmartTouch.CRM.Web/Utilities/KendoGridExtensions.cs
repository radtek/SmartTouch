using Kendo.Mvc.UI;
using Kendo.Mvc.UI.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartTouch.CRM.Web.Utilities
{
    public static class KendoGridExtensions
    {
        public static GridBoundColumnBuilder<TModel> DisplayNameTitle<TModel>(this GridBoundColumnBuilder<TModel> builder) where TModel : class, new()
        {
            // Create an adapter to access the typed grid column
            // (which contains the Expression)
            Type adapterType = typeof(GridBoundColumnAdapter<,>)
             .MakeGenericType(typeof(TModel), builder.Column.MemberType);
            IGridBoundColumnAdapter adapter =
             (IGridBoundColumnAdapter)Activator.CreateInstance(adapterType);

            // Use the adapter to get the title and set it
            return builder.Title(adapter.GetDisplayName(builder.Column));
        }

        private interface IGridBoundColumnAdapter
        {
            string GetDisplayName(IGridBoundColumn column);
        }

        private class GridBoundColumnAdapter<TModel, TValue>
            : IGridBoundColumnAdapter where TModel : class, new()
        {
            public string GetDisplayName(IGridBoundColumn column)
            {
                // Get the typed bound column
                GridBoundColumn<TModel, TValue> boundColumn =
                    column as GridBoundColumn<TModel, TValue>;
                if (boundColumn == null) return String.Empty;

                // Create the appropriate HtmlHelper and use it to get the display name
                HtmlHelper<TModel> helper = HtmlHelpers.For<TModel>(
                    boundColumn.Grid.ViewContext,
                    boundColumn.Grid.ViewData,
                    new RouteCollection());
                return helper.For(boundColumn.Expression).ToString();
            }
        }
    }
}