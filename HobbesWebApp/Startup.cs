using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HobbesWebApp.Startup))]
namespace HobbesWebApp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
