using System.Web;
using System.Web.Optimization;

namespace SASA.SAMS.HMI.WEB {
    public class BundleConfig {
        // 如需統合的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles) {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-1.12.1.custom/jquery-ui.min.js",
                        "~/Scripts/jquery-timepicker-master/jquery.timepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好可進行生產時，請使用 https://modernizr.com 的建置工具，只挑選您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/ASRS/global.css",
                    "~/Content/bootstrap.css",
                    "~/Scripts/jquery-ui-1.12.1.custom/jquery-ui.*",
                    "~/Scripts/jquery-timepicker-master/jquery.timepicker.min.css",
                    "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/Global/js").Include(
                      "~/Scripts/ASRS/global.js"));

            //Asrs Warehouse
            bundles.Add(new StyleBundle("~/ASRS/Warehouse/css").Include(
                "~/Content/ASRS/AsrsWarehouse*"
                ));
            bundles.Add(new ScriptBundle("~/ASRS/Warehouse/js").Include(
                "~/Scripts/ASRS/AsrsWarehouse*"
                ));
            //Asrs PFD
            bundles.Add(new StyleBundle("~/ASRS/PFD/css").Include(
                "~/Content/ASRS/AsrsPFD*"
                ));
            bundles.Add(new ScriptBundle("~/ASRS/PFD/js").Include(
                "~/Scripts/ASRS/AsrsPFD*"
                ));
        }
    }
}
