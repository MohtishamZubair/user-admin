using Saas.UserManage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Helpers;

namespace Saas.UserManage.Helper
{
    class WebHelper
    {
        static List<string> DemoCountries = new List<string>(new[] { "pakistan" });

        internal static bool IsRequestfrom(string countryName, string ipAddress)
        {
            return GetIpInfo(ipAddress).ToLowerInvariant().IndexOf(countryName.ToLowerInvariant()) > -1;
        }

        internal static string GetIpInfo(string ipAddress)
        {
            string responseInfo = string.Empty;

            if (!System.Web.HttpContext.Current.Request.IsLocal)
            {
                try
                {
                    using (WebClient proxy = new WebClient())
                    {
                        //responseInfo = proxy.DownloadString(string.Format("http://api.hostip.info/get_html.php?ip={0}",ipAddress));                
                        responseInfo = proxy.DownloadString(string.Format("http://www.freegeoip.net/json/{0}", ipAddress));
                    }
                }
                catch (Exception) { }
            }
            else
            {
                 responseInfo = "{'ip':'::1','country_code':'','country_name':'pakistan','region_code':'','region_name':'','city':'','zip_code':'','time_zone':'','latitude':0,'longitude':0,'metro_code':0}";
            }
            return responseInfo;
        }

        internal static void Clear()
        {
            DemoCountries.Clear();
        }

        internal static string Encrypt(string id)
        {
            return new string(id.Reverse().ToArray());
        }

        internal static string Decrypt(string id)
        {
            return Encrypt(id);
        }

        internal static bool IsDemoAllowed
        {
            get
            {
                bool isAllowed = false;
                var demoSession = GetfromSession(AppConstant.IS_DEMO);
                if (demoSession != null)
                {
                    isAllowed = (bool)demoSession;
                }
                else
                {
                    isAllowed = DemoCountries.Contains(RequestCountry.ToLowerInvariant());
                    PutIntoSession(AppConstant.IS_DEMO, isAllowed);
                }
                return isAllowed;
            }
        }

        public static string RequestCountry
        {
            get
            {
                string country = string.Empty;
                var countrySession = GetfromSession(AppConstant.COUNTRY);
                if (countrySession != null)
                {
                    country = countrySession.ToString();
                }
                else
                {
                    country = RequestData.Country;
                    PutIntoSession(AppConstant.COUNTRY, country);
                }
                return country;
            }
        }

        public static RequestInfo RequestData
        {
            get
            {
                RequestInfo  requestInfo  ;
                var requestInfoSession = GetfromSession(AppConstant.REQUEST_INFO);
                if (requestInfoSession != null)
                {
                    requestInfo = requestInfoSession as RequestInfo;
                }
                else
                {
                    requestInfo = GetRequestInfo();
                    PutIntoSession(AppConstant.REQUEST_INFO, requestInfo);
                }
                
                return requestInfo;
            }
        }

        private static RequestInfo GetRequestInfo()
        {
            string infoRawData = GetIpInfo(System.Web.HttpContext.Current.Request.UserHostAddress);
            dynamic data = Json.Decode(infoRawData);
            var requestInfo = new RequestInfo(data);
            return requestInfo;            
        }

        private static object GetfromSession(string sessionKey)
        {
            return System.Web.HttpContext.Current.Session[sessionKey];
        }

        private static void PutIntoSession(string sessionKey, object sessionValue)
        {
            System.Web.HttpContext.Current.Session[sessionKey] = sessionValue;
        }

    }
}