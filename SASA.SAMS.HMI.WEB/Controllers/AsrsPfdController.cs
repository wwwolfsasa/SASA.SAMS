using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SASA.SAMS.HMI.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SASA.SAMS.HMI.WEB.Controllers {
    public class AsrsPfdController:Controller {
        // GET: AsrsPid
        public ActionResult Index() {
            return View();
        }

        /// <summary>
        /// 新增/修改 裝置
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> EditDevice(string device = null, string alert = null) {
            ViewBag.Title = (device is null) ? "新增裝置" : $"編輯 裝置 [{device}]";
            if (alert != null)
                TempData["ALERT"] = alert;

            AsrsPfdEditModel model = new AsrsPfdEditModel();
            //裝置種類
            var response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest("http://localhost:32000/sams/device/type"));
            if (response != null && response["isSuccess"].Value<bool>()) {
                var tmp = response["Data"].Value<JArray>();
                model.DeviceTypeList = tmp.Select(item => {
                    return new SelectListItem() {
                        Text = item["Name"].Value<string>(),
                        Value = item["Value"].Value<string>()
                    };
                }).ToList();
            } else {
                model.DeviceTypeList = new List<SelectListItem>();
            }
            //裝置清單
            response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest("http://localhost:32000/sams/device/list"));
            if (response != null && response["isSuccess"].Value<bool>()) {
                model.OtherDeiviceList = response["Data"].Value<JArray>();
            }

            return View(model);
        }
        /// <summary>
        /// 新增/修改 裝置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditDevice(AsrsPfdEditModel model) {
            var response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.POSTRequest("http://localhost:32000/sams/device/edit", JsonConvert.SerializeObject(model)));
            if (response != null && response["isSuccess"].Value<bool>()) {
                return RedirectToAction("Index", "AsrsPfd");
            } else {
                return await EditDevice(alert: response["Status"].Value<string>());
            }
        }


    }
}