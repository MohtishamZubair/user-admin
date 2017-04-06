using Saas.UserManage.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Saas.UserManage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string demoRequest = Request.QueryString["demo-start"];
            if (demoRequest != null && WebHelper.IsDemoAllowed)
            {
                demoRequest = demoRequest.ToLowerInvariant();
               
                if (demoRequest!= "off")
                {
                    Request.RequestContext.HttpContext.Application["enable-demo"] = true;
                }
                else
                {
                    Request.RequestContext.HttpContext.Application.Remove("enable-demo");
                }
            }           
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void GetInfo() {
            var serverForwardX = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string serverForwardHeader = "not present";
            if (serverForwardX != null)
            {
                serverForwardHeader = serverForwardX.ToString();
            }

            string countryInfo = WebHelper.GetIpInfo(HttpContext.Request.UserHostAddress);
            Response.Write(
                string.Format(
                    @"language is :<br/>language : {0}
                       <br /> ui culture : {1} 
                       <br />host address  {2}
                       <br /> server forward variable:  {3}
                       <br /> globlization region  {4}  
                        <br /> globlization region  {5} 
                        <br /> Is request from Pakistan  {6}  <hr />",
                    Request.UserLanguages[0],
                    Thread.CurrentThread.CurrentUICulture.DisplayName,
                    HttpContext.Request.UserHostAddress,
                    serverForwardHeader,
                    RegionInfo.CurrentRegion.DisplayName,
                    countryInfo,
                    WebHelper.IsDemoAllowed
                    )
                );
        }        
    }
}