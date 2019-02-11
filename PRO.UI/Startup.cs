using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PRO.UI.Startup))]
namespace PRO.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //
            ConfigureAuth(app);
        }
    }
}
