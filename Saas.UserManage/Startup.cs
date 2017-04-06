using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Saas.UserManage.Startup))]
namespace Saas.UserManage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
