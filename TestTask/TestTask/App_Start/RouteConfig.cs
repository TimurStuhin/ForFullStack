using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TestTask
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "EditTeacher",
                url: "Home/EditTeacher/{userid}",
                defaults: new { controller = "Home", action = "EditTeacher", userid = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "EditStudent",
                url: "Home/EditStudent/{userid}",
                defaults: new { controller = "Home", action = "EditStudent", userid = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ListStudent",
                url: "{controller}/{action}/{filtering}/{page}",
                defaults: new { controller = "Home", action = "ListStudent", filtering = UrlParameter.Optional, page = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ListTeacher",
                url: "{controller}/{action}/{filtering}",
                defaults: new { controller = "Home", action = "ListTeacher", filtering = UrlParameter.Optional }
            );


        }
    }
}
