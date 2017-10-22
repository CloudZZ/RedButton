using System.Web.Mvc;
using System.Web.Routing;

namespace RedButton
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Default", 
                            "{action}/{organization}/{key}", 
                            new { controller = "RedButton", action = "Panic", organization = UrlParameter.Optional, key = UrlParameter.Optional });
        }
    }
}
