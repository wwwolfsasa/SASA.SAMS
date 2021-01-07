using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASA.SAMS.Warehouse.Manager {
    public class Events {
        /// <summary>
        /// 訊息事件
        /// </summary>
        /// <param name="message"></param>
        public delegate void MessageHandle(string message);
    }
}
