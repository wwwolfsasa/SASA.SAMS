using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SASA.SAMS.HMI.WEB.Models;
using SASA.SAMS.PFD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SASA.SAMS.HMI.WEB.Controllers {
    public class AsrsPfdController:Controller {
        // GET: AsrsPid
        public async Task<ActionResult> Index() {
            List<AsrsPfdModel> models = new List<AsrsPfdModel>();

            var response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest("http://localhost:32000/sams/device/list"));
            if (response != null && response["isSuccess"].Value<bool>()) {
                var devices = response["Data"].ToObject<List<PfdStructure.Device>>();
                devices?.ForEach(d => {
                    models.Add(new AsrsPfdModel() {
                        DeviceId = d.Id,
                        DeviceName = d.Name,
                        DeviceActive = d.Enable,
                        DeviceType = d.Type,
                        PositionX = d.PositionX,
                        PositionY = d.PositionY,
                        DeiviceConnectList = d.ConnectItems
                    });
                });
            }


            return View(models);
        }

        /// <summary>
        /// 新增/修改 裝置
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> EditDevice(string device = null, string alert = null) {
            ViewBag.Title = (device is null) ? "新增裝置" : $"編輯 裝置 [{device}]";
            if (alert != null)
                TempData["ALERT"] = alert;

            AsrsPfdModel model = new AsrsPfdModel();
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
            //物流方向
            response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest("http://localhost:32000/sams/device/direction"));
            if (response != null && response["isSuccess"].Value<bool>()) {
                var tmp = response["Data"].Value<JArray>();
                model.DeviceDirectionList = tmp.Select(item => {
                    return new SelectListItem() {
                        Text = item["Name"].Value<string>(),
                        Value = item["Value"].Value<string>()
                    };
                }).ToList();
            } else {
                model.DeviceDirectionList = new List<SelectListItem>();
            }
            //目前裝置
            if (device != null) {
                response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest($"http://localhost:32000/sams/device/get/{device}"));
                if (response != null && response["isSuccess"].Value<bool>()) {
                    PfdStructure.Device tmpDevice = response["Data"].ToObject<PfdStructure.Device>();
                    model.OrgDeviceId = device;
                    model.DeviceId = tmpDevice.Id;
                    model.DeviceName = tmpDevice.Name;
                    model.DeviceType = tmpDevice.Type;
                    model.DeviceActive = tmpDevice.Enable;
                    model.PositionX = tmpDevice.PositionX;
                    model.PositionY = tmpDevice.PositionY;
                    model.DeiviceConnectList = tmpDevice.ConnectItems;
                }
            }
            //裝置清單
            response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest("http://localhost:32000/sams/device/list"));
            if (response != null && response["isSuccess"].Value<bool>()) {
                model.DeiviceList = response["Data"].ToObject<List<PfdStructure.ConnectItem>>();
            }
            //校正連接
            if (model.DeiviceList != null) {
                //移除自己
                if (device != null) {
                    var self = model.DeiviceList.Where(d => d.Id.Equals(device))?.FirstOrDefault();
                    model.DeiviceList.Remove(self);
                }
                //
                model.DeiviceConnectList?.ForEach(c => {
                    var conn = model.DeiviceList.Where(d => d.Id.Equals(c.Id))?.FirstOrDefault();
                    if (conn != null) {
                        conn.isConnect = c.isConnect;
                        conn.Direction = c.Direction;
                    }
                });
            }

            return View(model);
        }
        /// <summary>
        /// 新增/修改 裝置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditDevice(AsrsPfdModel model) {
            var response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.POSTRequest("http://localhost:32000/sams/device/edit", JsonConvert.SerializeObject(model)));
            if (response != null && response["isSuccess"].Value<bool>()) {
                return RedirectToAction("Index", "AsrsPfd");
            } else {
                return await EditDevice(alert: response["Status"].Value<string>());
            }
        }
        /// <summary>
        /// 刪除裝置
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<ActionResult> RemoveDevice(string device, string type) {
            var response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.POSTRequest($"http://localhost:32000/sams/device/remove/{type}/{device}", ""));
            if (response != null && response["isSuccess"].Value<bool>()) {
                return RedirectToAction("Index", "AsrsPfd");
            } else {
                return await EditDevice(alert: response["Status"].Value<string>());
            }
        }
    }
}