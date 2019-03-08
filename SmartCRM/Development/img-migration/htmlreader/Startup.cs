using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(htmlreader.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace htmlreader
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
