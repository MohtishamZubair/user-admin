using Saas.UserManage.Helper;
using System.Text;
using System.Web.Mvc;

namespace Saas.UserManage.filters
{
    public class EnableDemo : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (filterContext.HttpContext.Application["enable-demo"] != null &&  WebHelper.IsDemoAllowed)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<script type=\"text/javascript\" defer=\"defer\">\n\t");
                sb.Append("var enableDemo = true;");
                sb.Append("\n\t</script>\n");
                filterContext.Controller.ViewBag.StartupScript = sb.ToString();
            }       
        }        
    }
}

