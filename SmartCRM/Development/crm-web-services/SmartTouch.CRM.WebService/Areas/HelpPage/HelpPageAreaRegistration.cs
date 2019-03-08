using System.Web.Http;
using System.Web.Mvc;

namespace SmartTouch.CRM.WebService.Areas.HelpPage
{
    /// <summary>
    /// for Help Page Area Registration
    /// </summary>
    public class HelpPageAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// AreaName is a property for HelpPageAreaRegistration
        /// </summary>
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }

        /// <summary>
        /// for register route
        /// </summary>
        /// <param name="context">context</param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HelpPage_Default",
                "Help/{action}/{apiId}",
                new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}