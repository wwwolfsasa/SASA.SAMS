using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASA.SAMS.Warehouse {
    public class ShelfHelper {
        public class SelfHelperDescription :Attribute {
            public string Description { get; set; }
        }

        public enum CELL_TYPE {
            [SelfHelperDescription(Description = "無")]
            NAN = 0,
            /// <summary>
            /// 代驗區
            /// </summary>
            [SelfHelperDescription(Description = "代驗區")]
            QN = 1,
            /// <summary>
            /// 棧板區
            /// </summary>
            [SelfHelperDescription(Description = "棧板區")]
            PL = 2,
            /// <summary>
            /// 良品區
            /// </summary>
            [SelfHelperDescription(Description = "良品區")]
            QY = 3,
            /// <summary>
            /// 冷區
            /// </summary>
            [SelfHelperDescription(Description = "冷區")]
            CA = 4,
            /// <summary>
            /// 備料區
            /// </summary>
            [SelfHelperDescription(Description = "備料區")]
            PA = 5
        }

        public enum CELL_STATUS {
            /// <summary>
            /// 空庫位
            /// </summary>
            [SelfHelperDescription(Description = "空庫位")]
            EMPTY = 0,
            /// <summary>
            /// 實庫位
            /// </summary>
            [SelfHelperDescription(Description = "實庫位")]
            STORED = 1,
            /// <summary>
            /// 預約入
            /// </summary>
            [SelfHelperDescription(Description = "預約入")]
            REQUEST_IN = 2,
            /// <summary>
            /// 預約出
            /// </summary>
            [SelfHelperDescription(Description = "預約出")]
            REQUEST_OUT = 3
        }


        public static T GetEnumByIndex<T>(int index) {
            T[] e = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            return e[index];
        }

    }

}
