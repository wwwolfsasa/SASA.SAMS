using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SASA.SAMS.PFD {

    /// <summary>
    /// 裝置種類
    /// </summary>
    public enum DeviceType {
        /// <summary>
        /// Port 口
        /// </summary>
        [AttributeItem("Port 口")]
        [SqlTable(Table = "port_node", PK = "port_id")]
        Port = 0,
        /// <summary>
        /// Crane
        /// </summary>
        [AttributeItem("Crane")]
        [SqlTable(Table = "crane_node", PK = "crane_id")]
        Crane = 1,
        /// <summary>
        /// 輸送帶
        /// </summary>
        [AttributeItem("輸送帶")]
        [SqlTable(Table = "cv_node", PK = "cv_id")]
        CV = 2,
        /// <summary>
        /// 架子
        /// </summary>
        [AttributeItem("架子")]
        [SqlTable(Table = "shelf_info", PK = "shelf_row")]
        Shelf = 3
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
        OUT = 2,
        /// <summary>
        /// 不可輸入 不可輸出
        /// </summary>
        [AttributeItem("不可輸入 不可輸出")]
        NAN = 3
    }

    public class PfdStructure {
        /// <summary>
        /// MongoDB Device DB
        /// </summary>
        public const string MongoDbName = "DeviceInfo";
        /// <summary>
        /// MongoDB Device Table
        /// </summary>
        public const string MongoTableName = "Device";

        /// <summary>
        /// 裝置
        /// </summary>
        public class Device {
            /// <summary>
            /// Mongo Id
            /// </summary>
            [BsonId]
            public ObjectId MongoId { get; set; }
            /// <summary>
            /// 裝置編號
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// 裝置名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 裝置種類
            /// </summary>
            public string Type { get; set; }
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
            /// 裝置編號
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// 物流方向 <seealso cref="DeviceDirection"/>
            /// </summary>
            public string Direction { get; set; }
        }
    }
}
