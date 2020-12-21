using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KM.ASRS.HMI.WEB.Models {
    public class CostumCellTypeModel {
        [DisplayName("自訂區域名稱")]
        [Required]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "字數需介於 1~5 個字")]
        [Database("type_id")]
        public string CellTypeId { get; set; }
        [DisplayName("自訂區域顏色")]
        [Database("color")]
        public string CellTypeColor { get; set; }
    }
}