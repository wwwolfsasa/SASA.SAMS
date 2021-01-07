using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SASA.SAMS.HMI.WEB.Models {
    public class DatabaseAttribute :Attribute {

        public string ColumnName { get; set; }

        public DatabaseAttribute(string ColumnName) {
            this.ColumnName = ColumnName;
        }


    }
}