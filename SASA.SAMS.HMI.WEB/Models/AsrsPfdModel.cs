using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SASA.SAMS.HMI.WEB.Controllers;
using SASA.SAMS.PFD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SASA.SAMS.HMI.WEB.Models {
    public class AsrsPfdModel {
        /// <summary>
        /// 裝置編號
        /// </summary>
        [Required(ErrorMessage = "裝置編號 不可為空")]
        [DisplayName("裝置編號")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "編號必須介於 1~5 個字元")]
        public string DeviceId { get; set; }
        /// <summary>
        /// 裝置名稱
        /// </summary>
        [DisplayName("裝置名稱")]
        public string DeviceName { get; set; }
        /// <summary>
        /// 裝置種類
        /// </summary>
        [Required(ErrorMessage = "裝置種類 不可為空")]
        [DisplayName("裝置種類")]
        public string DeviceType { get; set; }
        /// <summary>
        /// 裝置種類清單
        /// </summary>
        public IEnumerable<SelectListItem> DeviceTypeList { get; set; }
        /// <summary>
        /// 物流方向清單
        /// </summary>
        public IEnumerable<SelectListItem> DeviceDirectionList { get; set; }
        /// <summary>
        /// 裝置啟用
        /// </summary>
        [DisplayName("裝置啟用")]
        public bool DeviceActive { get; set; }
        /// <summary>
        /// 裝置清單
        /// </summary>
        [DisplayName("裝置清單")]
        public List<PfdStructure.ConnectItem> DeiviceList { get; set; }
        /// <summary>
        /// 裝置連接清單
        /// </summary>
        [DisplayName("裝置連接清單")]
        public List<PfdStructure.ConnectItem> DeiviceConnectList { get; set; }
    }
}