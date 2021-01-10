using iBoxDB.LocalServer;
using SASA.SAMS.Logger;
using SASA.SAMS.PFD;
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
using System.Data.SqlClient;

namespace SASA.SAMS.Warehouse.Manager {
    public partial class WarehouseController:ApiController {
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


        public static SqlConnection MSSQL { get; set; }
        /// <summary>
        /// MSSQL IP 位址
        /// </summary>
        public string MSSQL_IP { get; private set; }
        /// <summary>
        /// MSSQL SQL 具名
        /// </summary>
        public string MSSQL_NAME { get; private set; }
        /// <summary>
        /// MSSQL 連線資料庫名稱
        /// </summary>
        public string MSSQL_DB { get; private set; }
        /// <summary>
        /// MSSQL 使用者
        /// </summary>
        public string MSSQL_USER { get; private set; }
        /// <summary>
        /// MSSQL 密碼
        /// </summary>
        public string MSSQL_PWD { get; private set; }
        /// <summary>
        /// MSSQL 連線字串
        /// </summary>
        public string MSSQL_CONN { get; private set; }

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

            this.MSSQL_IP = config.AppSettings.Settings["MSSQL_IP"].Value;
            this.MSSQL_NAME = config.AppSettings.Settings["MSSQL_NAME"].Value;
            this.MSSQL_DB = config.AppSettings.Settings["MSSQL_DB"].Value;
            this.MSSQL_USER = config.AppSettings.Settings["MSSQL_USER"].Value;
            this.MSSQL_PWD = config.AppSettings.Settings["MSSQL_PWD"].Value;
            this.MSSQL_CONN = $"Data Source={this.MSSQL_IP}\\{this.MSSQL_NAME};Initial Catalog={this.MSSQL_DB};Persist Security Info=False;User ID={this.MSSQL_USER };Password={this.MSSQL_PWD };Connect Timeout=0;Pooling=true;Max Pool Size=100;Min Pool Size=5";
        }

        #region WebApi Server
        /// <summary>
        /// 啟動 WebApi Server
        /// </summary>
        public void StartServer() {
            try {
                MSSQL = new SqlConnection(this.MSSQL_CONN);
                MSSQL.Open();
                //
                Log.Write(Log.State.INFO, Log.App.WAREHOUSE, $"資料庫連線成功");
            } catch (Exception e) {
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, $"資料庫連線失敗, 失去記錄基本資料功能\r\n{e.Message}");
                MSSQL.Dispose();
            }

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

            try {
                MSSQL?.Close();
                Log.Write(Log.State.INFO, Log.App.WAREHOUSE, $"資料庫斷線成功");
            } catch (Exception e) {
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, $"資料庫斷線失敗\r\n{e.Message}");
                MSSQL?.Dispose();
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
                MSSQL?.Dispose();
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
        /// Pallet DB
        /// </summary>
        private string PalletRoot { get => "PalletCollect"; }
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
        /// 棧板資訊
        /// </summary>
        private string PalletInfoTableName { get => "table_pallet_info"; }

        /// <summary>
        /// 初始化 Warehouse DB
        /// </summary>
        public void OpenWarehouse() {
            StringBuilder query = new StringBuilder();

            if (this.isMongo) {
                MongoClient = new MongoClient($"mongodb://{this.MongoIP}:{this.MongoPort}");
                IMongoDatabase MongoHouse = null;
                //cell
                for (int r = 0; r < this.Row; r++) {
                    //Row DB
                    MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{r + 1}");
                    //檢查資料庫列數基本資料
                    query.AppendLine($"IF NOT EXISTS (SELECT * FROM [shelf_info] WHERE [shelf_row] = {r + 1}) INSERT INTO [shelf_info] ([shelf_row]) VALUES ({r + 1})");
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
                //pallet info
                MongoHouse = MongoClient.GetDatabase(this.PalletRoot);
                try { MongoHouse.CreateCollection(this.PalletInfoTableName); } catch { }
                //device info
                MongoHouse = MongoClient.GetDatabase(PfdStructure.MongoDbName);
                try { MongoHouse.CreateCollection(PfdStructure.MongoTableName); } catch { }
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

            if (MSSQL != null) {
                SqlCommand cmd = new SqlCommand(query.ToString(), MSSQL);
                try {
                    cmd.ExecuteNonQuery();
                } catch (Exception e) {
                    Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, $"資料庫 倉庫基本資料未建立\r\n{e.Message}");
                } finally {
                    query.Clear();
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

        #region Cell 相關
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

        /// <summary>
        /// 新增 棧板
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("km/pallet/new")]
        public object NewPallet([FromBody] JObject data) {
            ResponseStruct response = new ResponseStruct();

            try {
                Pallet pallet = data["PalletData"].ToObject<Pallet>();
                //
                response = (ResponseStruct)this.NewPallet(pallet);
            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = $"資料更新失敗, {e.Message}";
                response.Data = null;
            }

            return response;
        }
        /// <summary>
        /// 新增 棧板
        /// </summary>
        /// <param name="pallet"></param>
        /// <returns></returns>
        public object NewPallet(Pallet pallet) {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                IMongoDatabase MongoHouse = MongoClient.GetDatabase(this.PalletRoot);
                var palletTable = MongoHouse.GetCollection<Pallet>(this.PalletInfoTableName);

                var pallets = palletTable.AsQueryable().Where(p => p.PalletID.Equals(pallet.PalletID))?.ToList() ?? null;
                if (pallets is null || pallets.Count <= 0) {
                    if (pallet.PalletID.Trim().Equals(string.Empty)) {
                        response.isSuccess = false;
                        response.Status = $"棧板 ID 不可為空";
                        response.Data = null;
                    } else {
                        palletTable.InsertOne(pallet);

                        response.isSuccess = true;
                        response.Status = $"棧板 [{pallet.PalletID}] 新增成功";
                        response.Data = null;
                    }
                } else {
                    response.isSuccess = false;
                    response.Status = $"棧板 [{pallet.PalletID}] 已經存在";
                    response.Data = null;
                }
            } else {
                return new ResponseStruct() {
                    isSuccess = false,
                    Status = "iBoxDB 未支援",
                    Data = null
                };
            }

            return response;
        }
        /// <summary>
        /// 找到 棧板
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("km/pallet/get/all")]
        public object GetPallets() {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                IMongoDatabase MongoHouse = MongoClient.GetDatabase(this.PalletRoot);
                var palletTable = MongoHouse.GetCollection<Pallet>(this.PalletInfoTableName);

                var pallets = palletTable.AsQueryable().Select(p => p).OrderBy(p => p.PalletID)?.ToList() ?? null;
                if (pallets is null || pallets.Count <= 0) {
                    response.isSuccess = true;
                    response.Status = "棧板查詢成功";
                    response.Data = pallets;
                } else {
                    response.isSuccess = false;
                    response.Status = "無棧板";
                    response.Data = null;
                }
            } else {
                return new ResponseStruct() {
                    isSuccess = false,
                    Status = "iBoxDB 未支援",
                    Data = null
                };
            }

            return response;
        }
        /// <summary>
        /// 取得未存放之棧板
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("km/pallet/get/unstore")]
        public object GetUnstorePallets() {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                IMongoDatabase MongoHouse = null;
                //取得所有棧板
                MongoHouse = MongoClient.GetDatabase(this.PalletRoot);
                var palletTable = MongoHouse.GetCollection<Pallet>(this.PalletInfoTableName);
                var pallets = palletTable.AsQueryable().Select(p => p).OrderBy(p => p.PalletID)?.ToList() ?? null;
                //扣除 已存放之棧板
                for (int r = 0; r < this.Row; r++) {
                    //Row DB
                    MongoHouse = MongoClient.GetDatabase($"{this.WarehouseRoot}-{r + 1}");

                    for (int b = 0; b < this.Bay; b++) {
                        var bayTable = MongoHouse.GetCollection<ShelfBay>($"{this.BayTableName}{b + 1}");

                        try {
                            var onStoredPallet = bayTable.AsQueryable().Where(c => c.PalletID != null && !c.PalletID.Equals(string.Empty))?.Select(c => c.PalletID)?.ToList() ?? null;

                            onStoredPallet?.ForEach(p => {
                                var tmp = pallets.Where(tp => tp.PalletID.Equals(p))?.FirstOrDefault() ?? null;
                                if (tmp != null) {
                                    pallets.Remove(tmp);
                                }
                            });
                        } catch (Exception e) {
                            Log.Write(Log.State.WARN, Log.App.WAREHOUSE, e.Message);
                        }
                    }
                }

                if (pallets is null || pallets.Count <= 0) {
                    response.isSuccess = false;
                    response.Status = "無棧板";
                    response.Data = null;
                } else {
                    response.isSuccess = true;
                    response.Status = "棧板查詢成功";
                    response.Data = pallets;
                }
            } else {
                return new ResponseStruct() {
                    isSuccess = false,
                    Status = "iBoxDB 未支援",
                    Data = null
                };
            }

            return response;
        }
        /// <summary>
        /// 取得棧板上資訊
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("km/pallet/get/info/{palletId}")]
        public object GetPalletInfo(string palletId) {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                IMongoDatabase MongoHouse = MongoClient.GetDatabase(this.PalletRoot);
                var palletTable = MongoHouse.GetCollection<Pallet>(this.PalletInfoTableName);
                var pallet = palletTable.AsQueryable().Where(p => p.PalletID.Equals(palletId))?.FirstOrDefault();

                response.isSuccess = true;
                response.Status = $"資料讀取成功";
                response.Data = pallet;
            } else {
                return new ResponseStruct() {
                    isSuccess = false,
                    Status = "iBoxDB 未支援",
                    Data = null
                };
            }

            return response;
        }
        /// <summary>
        /// 保存棧板上資訊
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("km/pallet/modify/info")]
        public object SavePalletInfo([FromBody] JObject data) {
            ResponseStruct response = new ResponseStruct();

            if (this.isMongo) {
                if (MongoClient is null)
                    return new ResponseStruct() {
                        isSuccess = false,
                        Status = "倉庫未啟動",
                        Data = null
                    };

                try {
                    string pallet = data["PalletId"].ToObject<string>();
                    List<PalletItem> items = data["PalletItems"].ToObject<List<PalletItem>>();

                    IMongoDatabase MongoHouse = MongoClient.GetDatabase(this.PalletRoot);
                    var palletTable = MongoHouse.GetCollection<Pallet>(this.PalletInfoTableName);
                    palletTable.FindOneAndUpdateAsync(p => p.PalletID.Equals(pallet), Builders<Pallet>.Update.Set("Items", items)).Wait();
                    //
                    response.isSuccess = true;
                    response.Status = $"資料更新成功";
                    response.Data = null;
                } catch (Exception e) {
                    response.isSuccess = false;
                    response.Status = $"資料更新失敗, {e.Message}";
                    response.Data = null;
                }
            } else {
                return new ResponseStruct() {
                    isSuccess = false,
                    Status = "iBoxDB 未支援",
                    Data = null
                };
            }

            return response;
        }

        #endregion Cell 相關


        #region 設備 相關
        /// <summary>
        /// 取得設備種類
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("sams/device/type")]
        public object GetDeviceType() {
            ResponseStruct response = new ResponseStruct();

            DeviceType[] type = Enum.GetValues(typeof(DeviceType)).Cast<DeviceType>().ToArray();

            response.isSuccess = true;
            response.Status = $"查詢成功";
            response.Data = type.Select(t => {
                return new {
                    Name = t.GetType().GetMember(t.ToString()).First().GetCustomAttribute<AttributeItemAttribute>().Description,
                    Value = t.ToString()
                };
            });

            return response;
        }
        /// <summary>
        /// 取得連接設備物流方向
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("sams/device/direction")]
        public object GetDeviceDirection() {
            ResponseStruct response = new ResponseStruct();

            DeviceDirection[] dir = Enum.GetValues(typeof(DeviceDirection)).Cast<DeviceDirection>().ToArray();

            response.isSuccess = true;
            response.Status = $"查詢成功";
            response.Data = dir.Select(d => {
                return new {
                    Name = d.GetType().GetMember(d.ToString()).First().GetCustomAttribute<AttributeItemAttribute>().Description,
                    Value = d.ToString()
                };
            });

            return response;
        }
        /// <summary>
        /// 取得所有裝置 ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("sams/device/list")]
        public object GetAllDevice() {
            ResponseStruct response = new ResponseStruct();

            IMongoDatabase MongoHouse = MongoClient.GetDatabase(PfdStructure.MongoDbName);
            var deviceList = MongoHouse.GetCollection<PfdStructure.Device>(PfdStructure.MongoTableName);

            try {
                response.Data = deviceList.AsQueryable().Select(d => d).OrderBy(d => d.Type)?.ToList();

                response.isSuccess = true;
                response.Status = "查詢完成";
            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = "裝置查詢失敗";
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
            }

            return response;
        }
        /// <summary>
        /// 取得裝置
        /// </summary>
        /// <param name="deviceid"></param>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("*", "*", "*")]
        [Route("sams/device/get/{deviceid}")]
        public object GetDeviceById(string deviceid) {
            ResponseStruct response = new ResponseStruct();

            IMongoDatabase MongoHouse = MongoClient.GetDatabase(PfdStructure.MongoDbName);
            var deviceList = MongoHouse.GetCollection<PfdStructure.Device>(PfdStructure.MongoTableName);

            try {
                PfdStructure.Device device = deviceList.AsQueryable().Where(d => d.Id.Equals(deviceid))?.FirstOrDefault();
                if (device is null) {
                    response.isSuccess = false;
                    response.Status = "無此裝置";
                } else {
                    response.isSuccess = true;
                    response.Status = "裝置查詢成功";
                    response.Data = device;
                }
            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = "裝置查詢失敗";
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
            }

            return response;
        }
        /// <summary>
        /// 編輯 裝置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("sams/device/edit")]
        public object EditDeivce([FromBody] JObject data) {
            ResponseStruct response = new ResponseStruct();

            PfdStructure.Device device = new PfdStructure.Device();

            try {
                device.Id = data["DeviceId"].ToObject<string>();
                device.Name = data["DeviceName"].ToObject<string>();
                device.Type = data["DeviceType"].ToObject<string>();
                device.Enable = data["DeviceActive"].ToObject<bool>();
                device.ConnectItems = data["DeiviceList"].ToObject<List<PfdStructure.ConnectItem>>();

            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = "資料解讀錯誤";
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
                return response;
            }

            //mssql
            DeviceType type = (DeviceType)Enum.Parse(typeof(DeviceType), device.Type);
            string table = type.GetType().GetMember(type.ToString()).First().GetCustomAttribute<SqlTableAttribute>().Table;
            string pk = type.GetType().GetMember(type.ToString()).First().GetCustomAttribute<SqlTableAttribute>().PK;

            string query = $@"IF EXISTS (SELECT * FROM [{table}])
                                                    UPDATE [{table}] SET [{pk}]='{device.Id}', [is_active]='{device.Enable}' WHERE [{pk}]='{device.Id}'
                                                ELSE
                                                    INSERT INTO [{table}] ([{pk}],[is_active]) VALUES ('{device.Id}','{device.Enable}')";

            SqlCommand cmd = new SqlCommand(query, MSSQL);
            bool isMssqlSuccess = cmd.ExecuteNonQuery() > 0;

            //mongo
            bool isMongoSucces = false;
            IMongoDatabase MongoHouse = MongoClient.GetDatabase(PfdStructure.MongoDbName);
            var deviceList = MongoHouse.GetCollection<PfdStructure.Device>(PfdStructure.MongoTableName);

            try {
                var find = deviceList.AsQueryable().Where(d => d.Id.Equals(device.Id))?.ToArray();
                if (find.Length <= 0) {
                    deviceList.InsertOne(device);
                } else {
                    var update = Builders<PfdStructure.Device>.Update;
                    Array.ForEach(device.GetType().GetProperties(), p => {
                        deviceList.FindOneAndUpdateAsync(d => d.Id.Equals(device.Id), update.Set(p.Name, p.GetValue(device)));
                    });
                    //被連接的裝置
                    device.ConnectItems?.ForEach(c => {
                        var connDeviceConnectList = deviceList.AsQueryable().Where(d => d.Id.Equals(c.Id))?.FirstOrDefault()?.ConnectItems;
                        if (connDeviceConnectList is null)
                            connDeviceConnectList = new List<PfdStructure.ConnectItem>();

                        var mainDevice = connDeviceConnectList.Where(cdc => cdc.Id.Equals(device.Id))?.FirstOrDefault();
                        if (mainDevice is null) {
                            mainDevice = new PfdStructure.ConnectItem();
                            connDeviceConnectList.Add(mainDevice);
                        }

                        mainDevice.Id = device.Id;
                        mainDevice.isConnect = c.isConnect;
                        switch (Enum.Parse(typeof(DeviceDirection), c.Direction)) {
                            case DeviceDirection.IN:
                                mainDevice.Direction = DeviceDirection.OUT.ToString();
                                break;
                            case DeviceDirection.OUT:
                                mainDevice.Direction = DeviceDirection.IN.ToString();
                                break;
                            case DeviceDirection.INOUT:
                                mainDevice.Direction = DeviceDirection.INOUT.ToString();
                                break;
                            case DeviceDirection.NAN:
                                mainDevice.Direction = DeviceDirection.NAN.ToString();
                                break;
                        }

                        deviceList.FindOneAndUpdateAsync(d => d.Id.Equals(c.Id), Builders<PfdStructure.Device>.Update.Set("ConnectItems", connDeviceConnectList));
                    });
                }

                isMongoSucces = true;
            } catch (Exception e) {
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
            }

            //
            response.isSuccess = isMssqlSuccess || isMongoSucces;
            response.Status = (response.isSuccess) ? "裝置修改成功" : "裝置修改失敗";

            return response;
        }
        /// <summary>
        /// 設定位置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("*", "*", "*")]
        [Route("sams/device/set/position")]
        public object EditDeivcePosition([FromBody] JObject data) {
            ResponseStruct response = new ResponseStruct();

            string device = string.Empty;
            float x = 0, y = 0;
            try {
                device = data["DeviceId"].Value<string>();
                x = data["X"].Value<float>();
                y = data["Y"].Value<float>();
            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = "資料解讀錯誤";
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
                return response;
            }

            IMongoDatabase MongoHouse = MongoClient.GetDatabase(PfdStructure.MongoDbName);
            var deviceList = MongoHouse.GetCollection<PfdStructure.Device>(PfdStructure.MongoTableName);
            try {
                deviceList.FindOneAndUpdateAsync(d => d.Id.Equals(device), Builders<PfdStructure.Device>.Update.Set("PositionX", x)).Wait();
                deviceList.FindOneAndUpdateAsync(d => d.Id.Equals(device), Builders<PfdStructure.Device>.Update.Set("PositionY", y)).Wait();

                response.isSuccess = true;
                response.Status = "資料更新成功";
            } catch (Exception e) {
                response.isSuccess = false;
                response.Status = "資料更新錯誤";
                Log.Write(Log.State.ERROR, Log.App.WAREHOUSE, e.Message);
                return response;
            }

            return response;
        }
        #endregion 設備 相關

        #endregion Warehouse WebApi [基本操作]
    }
}
