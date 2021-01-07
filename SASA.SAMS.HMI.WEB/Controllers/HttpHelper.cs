using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SASA.SAMS.HMI.WEB.Controllers {
    public class HttpHelper {
        /// <summary>
        /// POST Data
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="jsonData">POST Data</param>
        public static async Task<string> POSTRequest(string httpAddress, string jsonData) {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string responseData = string.Empty;

            try {
                System.Net.Http.HttpResponseMessage response = await client.PostAsync(httpAddress,
                    new System.Net.Http.StringContent(jsonData, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                responseData = await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                responseData = jsonData;
            } finally {
                client.Dispose();
            }

            return responseData;
        }
        /// <summary>
        /// GET Data
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="data"></param>
        public static async Task<string> GETRequest(string httpAddress) {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string responseData = string.Empty;

            try {
                System.Net.Http.HttpResponseMessage response = await client.GetAsync(httpAddress);
                response.EnsureSuccessStatusCode();
                responseData = await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            } finally {
                client.Dispose();
            }

            return responseData;
        }
    }
}