using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SASA.SAMS.HMI.WEB.Models {
    public class DatabaseAttribute :Attribute {
        /// <summary>
        /// 資料表
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string ColumnName { get; set; }

        public DatabaseAttribute(string TableName, string ColumnName) {
            this.TableName = TableName;
            this.ColumnName = ColumnName;
        }

        public DatabaseAttribute(string ColumnName) {
            this.ColumnName = ColumnName;
        }


    }
}