using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ReckonTwo.Startup))]
namespace ReckonTwo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
