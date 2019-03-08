using System.Web;
using System.Web.Optimization;

namespace SmartTouch.CRM.WebService
{
    /// <summary>
    /// Creating BundleConFig
    /// </summary>
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        /// <summary>
        /// For Bundling Files
        /// </summary>
        /// <param name="bundles">bundles</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/bootstrap-responsive.css"));

            bundles.Add(new StyleBundle("~/Content/docs_css").Include(
                "~/Areas/HelpPage/Content/font-awesome.min.css",
                    "~/Content/bootstrap.min.css",
                    "~/Areas/HelpPage/HelpPage.css",
                    "~/Areas/HelpPage/Content/APIDocumentation.css"
                ));

            bundles.Add(new ScriptBundle("~/bundles/docs_jquery").Include(
                     "~/Scripts/jquery-{version}.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/docs_knockout").Include(
                     "~/Scripts/knockout-3.4.0.js",
                     "~/Scripts/knockout.mapping-latest.js",
                     "~/Scripts/ViewModels/APIDocumentation.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/docs_apidocvm").Include(
                "~/Scripts/ViewModels/APIDocumentation.js"
                ));
            //bundles.Add(new ScriptBundle("~/Content/api_doc_css").Include(
            //    "~/Content/tomorrow.css",
            //    "~/Areas/HelpPage/Content/SyntaxHighlighterCss/shcore.css",
            //    "~/Areas/HelpPage/Content/SyntaxHighlighterCss/shThemeDefault.css"));

            //bundles.Add(new ScriptBundle("~/bundles/api_docs_vm").Include(
            //    "~/Areas/HelpPage/Scripts/SyntaxHighlighter/shCore.js",
            //    "~/Areas/HelpPage/Scripts/SyntaxHighlighter/shBrushCSharp.js",
            //    "~/Areas/HelpPage/Scripts/SyntaxHighlighter/shBrushJScript.js",
            //    "~/Areas/HelpPage/Scripts/SyntaxHighlighter/shBrushPhp.js"
            //    ));

            bundles.Add(new ScriptBundle("~/bundles/api_docs_copy_vm").Include(
                 "~/Scripts/Prettify/prettify.js",
                "~/Areas/HelpPage/Scripts/alertify.min.js",
                "~/Areas/HelpPage/Scripts/application.js",
                 "~/Areas/HelpPage/Scripts/ZeroClipboard/ZeroClipboard.min.js"
                ));
        }
    }
}
