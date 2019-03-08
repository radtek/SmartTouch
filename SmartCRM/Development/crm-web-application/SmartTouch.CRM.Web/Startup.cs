using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmartTouch.CRM.Web.Startup))]
namespace SmartTouch.CRM.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
