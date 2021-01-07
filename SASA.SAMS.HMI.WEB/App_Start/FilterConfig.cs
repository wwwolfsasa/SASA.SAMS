using System.Web;
using System.Web.Mvc;

namespace SASA.SAMS.HMI.WEB {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
