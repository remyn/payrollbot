using System.Web.Mvc;
using System.Web.Routing;

namespace ReckonTwo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Comments",
                url: "comments",
                defaults: new { controller = "Home", action = "Comments" }
            );

            routes.MapRoute(
                name: "NewComment",
                url: "comments/new",
                defaults: new { controller = "Home", action = "AddComment" }
            );

            routes.MapRoute(
                name: "NewBotAnswer",
                url: "comments/addBotAnswer",
                defaults: new { controller = "Home", action = "AddBotAnswer" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "web", action = "login", id = UrlParameter.Optional }
            );
        }
    }
}
