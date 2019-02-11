using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PRO.Members.Startup))]
namespace PRO.Members
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
