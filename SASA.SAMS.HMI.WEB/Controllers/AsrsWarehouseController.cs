using SASA.SAMS.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SASA.SAMS.HMI.WEB.Models;
using System.Data.SqlClient;
using System.Reflection;

namespace SASA.SAMS.HMI.WEB.Controllers {
    public class AsrsWarehouseController :Controller {
        // GET: AsrsWarehouse
        public async Task<ActionResult> Index(int currentRow = 1) {
            ViewBag.CurrentRow = currentRow;
            var response = JsonConvert.DeserializeObject<JObject>(await HttpHelper.GETRequest("http://localhost:32000/km/warehouse/info"));
            ViewBag.Row = response["isSuccess"].Value<bool>() ? response["Data"]["ROW"].Value<int>() : 10;
            ViewBag.Bay = response["isSuccess"].Value<bool>() ? response["Data"]["BAY"].Value<int>() : 10;
            ViewBag.Level = response["isSuccess"].Value<bool>() ? response["Data"]["LEVEL"].Value<int>() : 10;

            AsrsWarehouseModel model = new AsrsWarehouseModel() {
                CostumCellTypeList = JsonConvert.DeserializeObject<List<CostumCellTypeModel>>(this.GetCostumCellTypeList())
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult NewCellType(AsrsWarehouseModel model) {

            using (SqlConnection sql = new SqlConnection(AsrsHMIApplication.MSSQL_CONNECT_STRING)) {
                try {
                    sql.Open();
                    string query = $"INSERT INTO [cell_costum_type] ([type_id], [color]) VALUES ('{model.CostumCellType.CellTypeId}', '{((model.CostumCellType.CellTypeColor.StartsWith("#")) ? model.CostumCellType.CellTypeColor.Substring(1) : model.CostumCellType.CellTypeColor)}');";
                    SqlCommand cmd = new SqlCommand(query, sql);

                    bool isSuccess = cmd.ExecuteNonQuery() > 0;
                    if (isSuccess) {
                        return RedirectToAction("Index", "AsrsWarehouse");
                    } else {
                        ViewBag.Error = "無新增動作";
                        return PartialView("~/Views/Shared/Error.cshtml");
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    ViewBag.Error = e.Message;
                    return PartialView("~/Views/Shared/Error.cshtml");
                } finally {
                    sql.Close();
                }
            }

        }


        public ActionResult DeleteCellType(string typeId) {
            using (SqlConnection sql = new SqlConnection(AsrsHMIApplication.MSSQL_CONNECT_STRING)) {
                try {
                    sql.Open();
                    string query = $"DELETE FROM [cell_costum_type] WHERE [type_id]='{typeId}';";
                    SqlCommand cmd = new SqlCommand(query, sql);

                    bool isSuccess = cmd.ExecuteNonQuery() > 0;
                    if (isSuccess) {
                        return RedirectToAction("Index", "AsrsWarehouse");
                    } else {
                        ViewBag.Error = "無刪除動作";
                        return PartialView("~/Views/Shared/Error.cshtml");
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    ViewBag.Error = e.Message;
                    return PartialView("~/Views/Shared/Error.cshtml");
                } finally {
                    sql.Close();
                }
            }
        }

        [HttpPost]
        [Route("asrs/costum/get/celltype")]
        public string GetCostumCellTypeList() {
            using (SqlConnection sql = new SqlConnection(AsrsHMIApplication.MSSQL_CONNECT_STRING)) {
                try {
                    sql.Open();
                    string query = $"SELECT * FROM [cell_costum_type];";
                    SqlCommand cmd = new SqlCommand(query, sql);
                    SqlDataReader sdr = cmd.ExecuteReader();

                    if (sdr.HasRows) {
                        List<CostumCellTypeModel> data = new List<CostumCellTypeModel>();

                        while (sdr.Read()) {
                            CostumCellTypeModel model = new CostumCellTypeModel();
                            Array.ForEach(model.GetType().GetProperties(), m => {
                                m.SetValue(model, sdr[m.GetCustomAttribute<DatabaseAttribute>().ColumnName]);
                            });

                            data.Add(model);
                        }

                        return JsonConvert.SerializeObject(data);
                    } else {
                        return null;
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return null;
                } finally {
                    sql.Close();
                }
            }
        }


    }
}