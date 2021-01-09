using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASA.SAMS.PFD {
    public class SqlTableAttribute:Attribute {
        /// <summary>
        /// 資料表
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// PK
        /// </summary>
        public string PK { get; set; }
    }
}
