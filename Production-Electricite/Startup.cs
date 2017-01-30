using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Production_Electricite.Startup))]
namespace Production_Electricite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app){}
    }
}
