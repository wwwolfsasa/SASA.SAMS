using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.ASRS.Warehouse {
    public partial class ShelfBay {
        /// <summary>
        /// 件號
        /// </summary>
        public string itnbr { get; set; }
        /// <summary>
        /// 保存最後期限
        /// </summary>
        public DateTime StoreDeadline { get; set; }

    }
}
