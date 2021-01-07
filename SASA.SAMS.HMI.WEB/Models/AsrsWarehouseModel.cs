using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KM.ASRS.HMI.WEB.Models {
    public class AsrsWarehouseModel {

        public List<CostumCellTypeModel> CostumCellTypeList { get; set; }
        public CostumCellTypeModel CostumCellType { get; set; }
    }
}