using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.ASRS.Warehouse {
    public partial class Pallet {
        /// <summary>
        /// Mongo Id
        /// </summary>
        [BsonId]
        public string MongoId { get; set; }
        /// <summary>
        /// 棧板 ID [PK]
        /// </summary>
        public string PalletName { get; set; }
        /// <summary>
        /// 棧板上貨物清單
        /// </summary>
        public List<PalletItem> Items { get; set; }
    }

    public partial class PalletItem {
        /// <summary>
        /// 物件名稱
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 存量
        /// </summary>
        public int Accumulation { get; set; }
    }
}
