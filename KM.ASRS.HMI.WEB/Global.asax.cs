using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace KM.ASRS.HMI.WEB {
    public class AsrsHMIApplication :System.Web.HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //
            var setting = System.Configuration.ConfigurationManager.AppSettings;

            MSSQL_CONNECT_STRING = $"Data Source={setting["MSSQL_IP"]}\\{setting["MSSQL_NAME"]};Initial Catalog={setting["MSSQL_DB_NAME"]};Persist Security Info=False;User ID={setting["MSSQL_USER_NAME"]};Password={setting["MSSQL_USER_PASSWORD"]};Connect Timeout=0;Pooling=true;Max Pool Size=100;Min Pool Size=5";
        }

        public static string MSSQL_CONNECT_STRING { get; private set; }
    }
}
