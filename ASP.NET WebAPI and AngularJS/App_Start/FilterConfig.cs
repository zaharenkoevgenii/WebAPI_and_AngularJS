using System.Web;
using System.Web.Mvc;

namespace ASP.NET_WebAPI_and_AngularJS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
