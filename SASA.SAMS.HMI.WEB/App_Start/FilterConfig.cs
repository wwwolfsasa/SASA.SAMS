using System.Web;
using System.Web.Mvc;

namespace KM.ASRS.HMI.WEB {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
