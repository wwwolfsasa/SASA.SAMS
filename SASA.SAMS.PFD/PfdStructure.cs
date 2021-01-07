using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASA.SAMS.PFD {
    public class PfdStructure {
        /// <summary>
        /// 裝置
        /// </summary>
        public class Device {
            /// <summary>
            /// 裝置名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 裝置種類
            /// </summary>
            public int Type { get; set; }
            /// <summary>
            /// 啟/禁用
            /// </summary>
            public bool Enable { get; set; }
            /// <summary>
            /// 連接裝置
            /// </summary>
            public List<ConnectItem> ConnectItems { get; set; }
        }

        /// <summary>
        /// 連接裝置
        /// </summary>
        public class ConnectItem {
            /// <summary>
            /// 裝置名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 裝置種類
            /// </summary>
            public int Type { get; set; }
            /// <summary>
            /// 物流方向 <seealso cref="DeviceDirection"/>
            /// </summary>
            public int Direction { get; set; }
        }


        
        /// <summary>
        /// 裝置種類
        /// </summary>
        public enum DeviceType {
            /// <summary>
            /// Port 口
            /// </summary>
            [AttributeItem("Port 口")]
            Port=0,
            /// <summary>
            /// Crane
            /// </summary>
            [AttributeItem("Crane")]
            Crane = 1,
            /// <summary>
            /// 輸送帶
            /// </summary>
            [AttributeItem("輸送帶")]
            CV =2,
            /// <summary>
            /// 架子
            /// </summary>
            [AttributeItem("架子")]
            Shelf =3
        }

        /// <summary>
        /// 物流方向
        /// </summary>
        public enum DeviceDirection {
            /// <summary>
            /// 雙向皆可
            /// </summary>
            [AttributeItem("雙向物流")]
            INOUT = 0,
            /// <summary>
            /// 只能輸入
            /// </summary>
            [AttributeItem("只能輸入")]
            IN = 1,
            /// <summary>
            /// 只能輸出
            /// </summary>
            [AttributeItem("只能輸出")]
            OUT = 2
        }
    }
}
