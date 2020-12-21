using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace KM.ASRS.Warehouse.Manager {
    public partial class WarehouseController: ApiController {
        #region  Warehouse WebApi [客製化操作]




        [HttpPost]
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("km/warehouse/test")]
        public object Test() {
            return new {
                TEST = DateTime.Now
            };
        }

        #endregion Warehouse WebApi [客製化操作]
    }
}
