using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmartTouch.CRM.WebService.Startup))]
namespace SmartTouch.CRM.WebService
{
    /// <summary>
    /// partial class for startup application 
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// for Configuration
        /// </summary>
        /// <param name="app">app</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}