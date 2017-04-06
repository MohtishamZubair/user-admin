using Saas.UserManage.filters;
using System.Web;
using System.Web.Mvc;

namespace Saas.UserManage
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new EnableDemo());
        }
    }
}
