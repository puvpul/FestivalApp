using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MazdaFestival
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_BeginRequest(object sender, EventArgs e)
        //{
        //    string[] ips = {"59.167.199.233", "127.0.0.1", "59.167.32.57", "::1"};

        //    string ip = Request.UserHostAddress;
            
        //    if (!ips.Contains(ip) && !Request.QueryString.AllKeys.Contains("B4D455"))
        //    {
        //        Response.Write("<html><body>403.6 IP ADDRESS REJECTED</body></html>");
        //        Response.End();
        //    }
        //}
    }
}
