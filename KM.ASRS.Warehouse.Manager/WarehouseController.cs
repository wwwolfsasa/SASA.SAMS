using iBoxDB.LocalServer;
using KM.ASRS.Logger;
using KM.MITSU.DE.Global;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

namespace KM.ASRS.Warehouse.Manager {
    public partial class WarehouseController :ApiController {
        /// <summary>
        /// 訊息事件
        /// </summary>
        public event Events.MessageHandle OnMessage;

        /// <summary>
        /// Port [32000]
        /// </summary>
        public int ServerPort { get; private set; }
        /// <summary>
        /// WebApi 核心
        /// </summary>
        private HttpSelfHostServer Server { get; set; }
        /// <summary>
        /// WebApi 廣域設定參數
        /// </summary>
        private HttpSelfHostConfiguration WebApiConfig { get; set; }
        /// <summary>
        /// 使用 MongoDB
        /// </summary>
        private bool isMongo { get; set; }

        /// <summary>
        /// 列數
        /// </summary>
        public int Row { get; private set; }
        /// <summary>
        /// 格數
        /// </summary>
        public int Bay { get; private set; }
        /// <summary>
        /// 層數
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public WarehouseController() {
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            this.ServerPort = (int.TryParse(config.AppSettings.Settings["WAREHOUSE_PORT"].Value, out int port)) ? port : 32000;
            //
            this.InitialConfig();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public WarehouseController(int port) {
            this.ServerPort = port;
            //
            this.InitialConfig();
        }

        /// <summary>
        /// 初始化-讀取倉庫大小
        /// </summary>
        private void InitialConfig() {
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            this.Row = (int.TryParse(config.AppSettings.Settings["ROW_COUNT"].Value, out int row)) ? row : 10;
            this.Bay = (int.TryParse(config.AppSettings.Settings["BAY_COUNT"].Value, out int bay)) ? bay : 10;
            this.Level = (int.TryParse(config.AppSettings.Settings["LEVEL_COUNT"].Value, out int level)) ? level : 10;

            this.isMongo = (bool.TryParse(config.AppSettings.Settings["USE_MONGO"].Value, out bool mongo)) ? mongo : false;
            if (this.isMongo) {
                this.MongoIP = config.AppSettings.Settings["MONGO_DB_IP"].Value;
                this.MongoPort = (int.TryParse(config.AppSettings.Settings["MONGO_DB_PORT"].Value, out int mongoPort)) ? mongoPort : 27017;
            }
        }

        #region WebApi Server
        /// <summary>
        /// 啟動 WebApi Server
        /// </summary>
        public void StartServer() {
            try {
                //
                this.OpenWarehouse();
                //啟動 WebApi
                this.WebApiConfig = new HttpSelfHostConfiguration($"http://localhost:{this.ServerPort}");
                this.WebApiConfig.EnableCors();
                this.WebApiConfig.MapHttpAttributeRoutes();
                //去除XML格式
                var appXmlType = this.WebApiConfig.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType.Equals("application/xml"));
                this.WebApiConfig.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
                //
                //回傳JSON格式
                this.WebApiConfig.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
                //this.WebApiConfig.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                this.Server = new HttpSelfHostServer(this.WebApiConfig);
                this.Server.OpenAsync().Wait();
                //
                Log.Write(Log.State.INFO, Log.App.WAREHOUSE, $"WebApi Server 啟動成功 [{this.ServerPort}]");
                OnMessage?.Invoke("WebApi Server 啟動成功");
            } catch (Exception e) {
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, $"WebApi Server 啟動失敗 [{this.ServerPort}],\r\n{e.Message}");
                this.StopServer();
            }
        }
        /// <summary>
        /// 關閉 WebApi Server
        /// </summary>
        public void StopServer() {
            try {
                this.CloseWarehouse();
                //
                this.Server?.CloseAsync().Wait();
                //
                Log.Write(Log.State.INFO, Log.App.WAREHOUSE, "WebApi Server 關閉成功");
                OnMessage?.Invoke("WebApi Server 關閉成功");
            } catch (Exception e) {
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, $"WebApi Server 關閉失敗, {e.Message}");
                OnMessage?.Invoke("WebApi Server 關閉失敗, 請查閱錯誤紀錄檔");
            }
        }
        /// <summary>
        /// 關閉 WebApi Server
        /// </summary>
        public void ClearServer() {
            try {
                this.CloseWarehouse();
                //
                this.Server?.Dispose();
                //
                Log.Write(Log.State.INFO, Log.App.WAREHOUSE, "History Service 關閉成功");
            } catch (Exception e) {
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, $"History Service 關閉失敗,\r\n{e.Message}");
            }
        }

        /// <summary>
        /// 回覆結構
        /// </summary>
        public struct ResponseStruct {
            /// <summary>
            /// 是否成功
            /// </summary>
            public bool isSuccess { get; set; }
            /// <summary>
            /// 狀態說明
            /// </summary>
            public string Status { get; set; }
            /// <summary>
            /// 回傳資料
            /// </summary>
            public object Data { get; set; }
        }
        #endregion WebApi Server

        #region Warehouse
        /// <summary>
        /// MongoDB IP
        /// </summary>
        private string MongoIP { get; set; }
        /// <summary>
        /// MongoDB Port
        /// </summary>
        private int MongoPort { get; set; }
        /// <summary>
        /// Warehouse in Mongo
        /// </summary>
        private static MongoClient MongoClient { get; set; }
        /// <summary>
        /// DB Root
        /// </summary>
        private string WarehouseRoot { get => "Warehouse"; }
        /// <summary>
        /// 倉庫資料庫 [row, DB]
        /// </summary>
        private static Dictionary<int, DB> ShelfRowDB { get; set; }
        /// <summary>
        /// Bay 資料表 [row, bay.level]
        /// </summary>
        private static Dictionary<int, AutoBox> ShelfBayTable { get; set; }
        /// <summary>
        /// Table Bay 前綴
        /// </summary>
        private string BayTableName { get => "table_bay_"; }

        /// <summary>
        /// 初始化 Warehouse DB
        /// </summary>
        public void OpenWarehouse() {
            if (this.isMongo) {
                MongoClient = new MongoClient($"mongodb://{this.MongoIP}:{this.MongoPort}");

                for (int r = 0; r < this.Row; r++) {
                    //Row DB
                    IMongoDatabase MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{r + 1}");
                    //
                    for (int b = 0; b < this.Bay; b++) {
                        //Bay table
                        try { MongoHouse.CreateCollection($"{this.BayTableName}{b + 1}"); } catch { }
                    }
                    //建立儲格 PK 重複略過
                    for (int b = 0; b < this.Bay; b++) {

                        for (int l = 0; l < this.Level; l++) {
                            try {
                                var bayTable = MongoHouse.GetCollection<ShelfBay>($"{this.BayTableName}{b + 1}");
                                var cellData = bayTable.AsQueryable().Where(c => c.Row.Equals(r + 1) && c.Bay.Equals(b + 1) && c.Level.Equals(l + 1))?.FirstOrDefault();
                                if (cellData is null) {
                                    bayTable.InsertOne(new ShelfBay() {
                                        Row = r + 1,
                                        Bay = b + 1,
                                        Level = l + 1,
                                        CellStatus = (int)ShelfHelper.CELL_STATUS.EMPTY,
                                        CellType = (string)ShelfHelper.CELL_TYPE.PL.ToString(),
                                        PalletID = string.Empty,
                                        Prohibit = true,
                                        SystemProhibit = true,
                                        UpdateTime = DateTime.Now,
                                        itnbr = string.Empty
                                    });
                                }
                            } catch (Exception e) {
                                Log.Write(Log.State.WARN, Log.App.WAREHOUSE, e.Message);
                            }
                        }
                    }
                }
            } else {
                System.IO.DirectoryInfo root = new System.IO.DirectoryInfo($"{GlobalString.GetInstallRoot}\\{this.WarehouseRoot}");
                if (!root.Exists)
                    root.Create();

                ShelfRowDB = new Dictionary<int, DB>();
                ShelfBayTable = new Dictionary<int, AutoBox>();

                //
                iBoxDB.LocalServer.DB.Root(root.ToString());

                for (int r = 0; r < this.Row; r++) {
                    //Row DB
                    DB dbRow = new DB(r + 1, root.ToString());
                    ShelfRowDB.Add(r, dbRow);
                    //
                    for (int b = 0; b < this.Bay; b++) {
                        //Bay table
                        dbRow.GetConfig().EnsureTable<ShelfBay>($"{this.BayTableName}{b + 1}", "Level");
                    }
                    //
                    ShelfBayTable.Add(r + 1, dbRow.Open());
                    //建立儲格 PK 重複略過
                    for (int b = 0; b < this.Bay; b++) {

                        for (int l = 0; l < this.Level; l++) {
                            try {
                                ShelfBayTable[r + 1].Insert($"{this.BayTableName}{b + 1}", new ShelfBay() {
                                    Row = r + 1,
                                    Bay = b + 1,
                                    Level = l + 1,
                                    CellStatus = (int)ShelfHelper.CELL_STATUS.EMPTY,
                                    CellType = ShelfHelper.CELL_TYPE.PL.ToString(),
                                    PalletID = string.Empty,
                                    Prohibit = true,
                                    SystemProhibit = true,
                                    UpdateTime = DateTime.Now,
                                    itnbr = string.Empty
                                });
                            } catch (Exception e) {
                                Log.Write(Log.State.WARN, Log.App.WAREHOUSE, e.Message);
                            }
                        }
                    }
                }
            }
            //
            Log.Write(Log.State.INFO, Log.App.WAREHOUSE, "倉庫準備完成");
        }
        /// <summary>
        /// 關閉 Warehouse DB
        /// </summary>
        public void CloseWarehouse() {
            ShelfRowDB?.Values?.ToList()?.ForEach(db => {
                db.Dispose();
            });
            //
            Log.Write(Log.State.INFO, Log.App.WAREHOUSE, "倉庫已關閉");
        }

        #endregion Warehouse

        #region Warehouse WebApi [基本操作]
        /// <summary>
        /// 倉庫訊息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("km/warehouse/info")]
        public object GetWarehouseInfo() {
            ResponseStruct response = new ResponseStruct();

            response.isSuccess = true;
            response.Status = $"倉庫訊息 查詢成功";
            response.Data = new {
                ROW = this.Row,
                BAY = this.Bay,
                LEVEL = this.Level,
                ServerPort = this.ServerPort
            };

            return response;
        }

        /// <summary>
        /// 取得儲格
        /// </summary>
        /// <param name="row"></param>
        /// <param name="bay"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("km/warehouse/cell/get/{row:int}/{bay:int}/{level:int}")]
        public object GetCell(int row, int bay, int level) {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                IMongoDatabase MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{row}");
                ShelfBay cell = MongoHouse.GetCollection<ShelfBay>($"{this.BayTableName}{bay}").AsQueryable().Where(c => c.Row.Equals(row) && c.Bay.Equals(bay) && c.Level.Equals(level))?.FirstOrDefault();

                response.isSuccess = !(cell is null);
                response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 查詢{((response.isSuccess) ? "成功" : "失敗")}";
                response.Data = cell;

            } else {
                if (ShelfRowDB is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                if (ShelfBayTable is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫資訊不完整",
                        Data = null
                    };

                try {
                    if (ShelfBayTable.ContainsKey(row)) {
                        ShelfBay cell = (from c in ShelfBayTable[row].Select<ShelfBay>($"from {this.BayTableName}{bay}") where c.Level.Equals(level) select c)?.FirstOrDefault();
                        //
                        response.isSuccess = true;
                        response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 查詢成功";
                        response.Data = cell;
                    } else {
                        response.isSuccess = false;
                        response.Status = $"倉庫不包含 第 {row} 列, 請確認設定檔是否正確";
                        response.Data = null;
                    }
                } catch (Exception e) {
                    response.isSuccess = false;
                    response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 查詢失敗, {e.Message}";
                    response.Data = null;
                    //
                    Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
                }
            }

            return response;
        }
        /// <summary>
        /// 取得一格上所有儲格
        /// </summary>
        /// <param name="row"></param>
        /// <param name="bay"></param>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("km/warehouse/cell/get/{row:int}/{bay:int}")]
        public object GetCellsAtBay(int row, int bay) {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                IMongoDatabase MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{row}");
                List<ShelfBay> cells = MongoHouse.GetCollection<ShelfBay>($"{this.BayTableName}{bay}").AsQueryable().Where(c => c.Row.Equals(row) && c.Bay.Equals(bay))?.ToList();

                response.isSuccess = !(cells is null);
                response.Status = $"儲格清單 [{row}]-[{bay}] 查詢{((response.isSuccess) ? "成功" : "失敗")}";
                response.Data = cells;

            } else {
                if (ShelfRowDB is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                if (ShelfBayTable is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫資訊不完整",
                        Data = null
                    };

                try {
                    if (ShelfBayTable.ContainsKey(row)) {
                        List<ShelfBay> cells = (from c in ShelfBayTable[row].Select<ShelfBay>($"from {this.BayTableName}{bay} order by Level asc") select c)?.ToList();
                        //
                        response.isSuccess = true;
                        response.Status = $"儲格清單 [{row}]-[{bay}] 查詢成功";
                        response.Data = cells;
                    } else {
                        response.isSuccess = false;
                        response.Status = $"倉庫不包含 第 {row} 列, 請確認設定檔是否正確";
                        response.Data = null;
                    }
                } catch (Exception e) {
                    response.isSuccess = false;
                    response.Status = $"儲格清單 [{row}]-[{bay}] 查詢失敗, {e.Message}";
                    response.Data = null;
                    //
                    Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
                }
            }

            return response;
        }
        /// <summary>
        /// 取得一排上所有儲格
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("km/warehouse/cell/get/{row:int}")]
        public object GetCellsAtBay(int row) {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                IMongoDatabase MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{row}");
                List<object> tmpBay = new List<object>();

                for (int b = 0; b < this.Bay; b++) {
                    List<ShelfBay> cells = MongoHouse.GetCollection<ShelfBay>($"{this.BayTableName}{b + 1}").AsQueryable().Where(c => c.Row.Equals(row) && c.Bay.Equals(b + 1))?.ToList();
                    tmpBay.Add(cells);
                }

                response.isSuccess = tmpBay.Count > 0;
                response.Status = $"儲格清單 [{row}] 查詢{((response.isSuccess) ? "成功" : "失敗")}";
                response.Data = tmpBay;
            } else {
                if (ShelfRowDB is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                if (ShelfBayTable is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫資訊不完整",
                        Data = null
                    };

                try {
                    if (ShelfBayTable.ContainsKey(row)) {
                        List<object> tmpBay = new List<object>();

                        for (int b = 0; b < this.Bay; b++) {
                            List<ShelfBay> cells = (from c in ShelfBayTable[row].Select<ShelfBay>($"from {this.BayTableName}{b + 1} order by Level asc") select c)?.ToList();
                            tmpBay.Add(cells);
                        }

                        //
                        response.isSuccess = true;
                        response.Status = $"儲格清單 [{row}] 查詢成功";
                        response.Data = tmpBay;
                    } else {
                        response.isSuccess = false;
                        response.Status = $"倉庫不包含 第 {row} 列, 請確認設定檔是否正確";
                        response.Data = null;
                    }
                } catch (Exception e) {
                    response.isSuccess = false;
                    response.Status = $"儲格清單 [{row}] 查詢失敗, {e.Message}";
                    response.Data = null;
                    //
                    Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
                }
            }

            return response;
        }
        /// <summary>
        /// 設置儲格狀態 [WebApi]
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("km/warehouse/cell/set/{row:int}/{bay:int}/{level:int}")]
        public object SetCell(int row, int bay, int level, [FromBody] JObject data) {
            ResponseStruct response = new ResponseStruct();

            try {
                ShelfBay cell = data["CellData"].ToObject<ShelfBay>();

                if (!this.isMongo && !ShelfBayTable.ContainsKey(row)) {
                    response.isSuccess = false;
                    response.Status = $"倉庫不包含 第 {row} 列, 請確認設定檔是否正確";
                    response.Data = null;
                    return response;
                }

                response = (ResponseStruct)this.SetCell(row, bay, level, cell);

            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 資料更新失敗, {e.Message}";
                response.Data = null;
                //
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
            }

            return response;
        }
        /// <summary>
        /// 設置儲格狀態
        /// </summary>
        /// <param name="row"></param>
        /// <param name="bay"></param>
        /// <param name="level"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public object SetCell(int row, int bay, int level, ShelfBay cell) {
            ResponseStruct response = new ResponseStruct();

            if (row.Equals(cell.Row) && bay.Equals(cell.Bay) && level.Equals(cell.Level)) {
                if (this.isMongo) {
                    if (MongoClient is null)
                        return new ResponseStruct() {
                            isSuccess = false,
                            Status = "倉庫未啟動",
                            Data = null
                        };

                    try {
                        IMongoDatabase MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{row}");
                        var bayTable = MongoHouse.GetCollection<ShelfBay>($"{this.BayTableName}{bay}");

                        Array.ForEach(cell.GetType().GetProperties(), cp => {
                            if (cp.GetCustomAttribute<BsonIdAttribute>() is null) {
                                bayTable.UpdateOneAsync(c => c.Row.Equals(cell.Row) && c.Bay.Equals(cell.Bay) && c.Level.Equals(cell.Level), Builders<ShelfBay>.Update.Set(cp.Name, cp.GetValue(cell, null))).Wait();
                            }
                        });

                        response.isSuccess = true;
                        response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 資料更新成功";
                        response.Data = null;
                    } catch (Exception e) {
                        response.isSuccess = false;
                        response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 資料更新失敗, {e.Message}";
                        response.Data = null;
                    }
                } else {
                    if (ShelfRowDB is null)
                        return new ResponseStruct() {
                            isSuccess = false,
                            Status = "倉庫未啟動",
                            Data = null
                        };

                    if (ShelfBayTable is null)
                        return new ResponseStruct() {
                            isSuccess = false,
                            Status = "倉庫資訊不完整",
                            Data = null
                        };


                    if (ShelfBayTable.ContainsKey(row)) {
                        response.isSuccess = ShelfBayTable[row].Update($"{this.BayTableName}{bay}", cell);
                        response.Status = $"儲格 [{row}]-[{bay}]-[{level}] 資訊更新成功";
                        response.Data = null;
                    } else {
                        response.isSuccess = false;
                        response.Status = $"倉庫不包含 第 {row} 列, 請確認設定檔是否正確";
                        response.Data = null;
                    }
                }
            } else {
                response.isSuccess = false;
                response.Status = $"更新失敗, 資料不一致";
                response.Data = null;
            }
            return response;
        }


        #endregion Warehouse WebApi [基本操作]
    }
}
