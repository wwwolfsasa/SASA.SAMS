using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.ASRS.Warehouse {
    public partial class ShelfBay {
        [BsonId]
        public ObjectId MongoCell { get; set; }
        /// <summary>
        /// 列數
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// 格數
        /// </summary>
        public int Bay { get; set; }
        /// <summary>
        /// 層數 [PK]
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 儲格種類 <seealso cref="ShelfHelper.CELL_TYPE"/>
        /// </summary>
        public string CellType { get; set; }
        /// <summary>
        /// 儲格狀態 <seealso cref="ShelfHelper.CELL_STATUS"/>
        /// </summary>
        public int CellStatus { get; set; }
        /// <summary>
        /// 棧板號碼 [FK]
        /// </summary>
        public string PalletID { get; set; }
        /// <summary>
        /// 使用者禁用
        /// </summary>
        public bool Prohibit { get; set; }
        /// <summary>
        /// 系統禁用
        /// </summary>
        public bool SystemProhibit { get; set; }
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
