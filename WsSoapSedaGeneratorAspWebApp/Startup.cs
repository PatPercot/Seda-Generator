using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WsSoapSedaGeneratorAspWebApp.Startup))]
namespace WsSoapSedaGeneratorAspWebApp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
