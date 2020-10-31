using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ServerApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ValuesController(IConfiguration config)
        {
            _config = config;
        }

        // GET api/values
        [HttpGet("GetItem")]
        public ActionResult<List<Item>> GetItem()
        {
            List<Item> items = new List<Item>();
            SqlConnection sqlConnection = new SqlConnection(_config.GetValue<string>("ConnectionString"));
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "getItemMasterdata";
            sqlCommand.Connection = sqlConnection;
            sqlConnection.Open();
            using (SqlDataReader oReader = sqlCommand.ExecuteReader())
            {
                while (oReader.Read())
                {
                    Item item = new Item();
                    item.ItemId = Convert.ToInt32(oReader["ItemId"]);
                    item.ItemName = oReader["ItemName"].ToString();
                    item.CategoryName = oReader["CategoryName"].ToString();
                    items.Add(item);
                }

                sqlConnection.Close();
            }
            return items;
        }

        // GET api/values/5
        [HttpGet("FindSuppliers")]
        public ActionResult<List<SupplierInfo>> FindSuppliers(int id)
        {
            List<SupplierInfo> suppliers = new List<SupplierInfo>();
            List<SupplierScoreInfo> supplierScores = new List<SupplierScoreInfo>();
            FindSupplierRequestModel item = new FindSupplierRequestModel();
            item.item_id = id;
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("http://127.0.0.1:9001")
            };
            var json = JsonConvert.SerializeObject(item);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("recommend"))
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
            HttpResponseMessage httpResponseMessage = client.SendAsync(request).Result;
            string output = String.Empty;
            output = httpResponseMessage.Content.ReadAsStringAsync().Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var ErrMsg = JsonConvert.DeserializeObject<dynamic>(httpResponseMessage.Content.ReadAsStringAsync().Result).ErrorMessage;
            }
            else
            {
                supplierScores = JsonConvert.DeserializeObject<List<SupplierScoreInfo>>(output);
            }
            string supplierIds = string.Empty;
            string FormattedsupplierIds = string.Empty;
            foreach (var element in supplierScores)
            {
                supplierIds = supplierIds +element.SupplierId.ToString()+",";
            }
            if (supplierIds.EndsWith(','))
            {
                FormattedsupplierIds = supplierIds.Remove(supplierIds.Length - 1, 1);
            }
            SqlConnection sqlConnection = new SqlConnection(_config.GetValue<string>("ConnectionString"));
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "getSupplierInfoById";
            sqlCommand.Connection = sqlConnection;
            sqlCommand.Parameters.AddWithValue("@SupplierIds", FormattedsupplierIds);
            sqlConnection.Open();
            using (SqlDataReader oReader = sqlCommand.ExecuteReader())
            {
                while (oReader.Read())
                {
                    SupplierInfo supplierInfo = new SupplierInfo();
                    supplierInfo.SupplierId = Convert.ToInt32(oReader["SupplierId"]);
                    supplierInfo.SupplierName = oReader["SupplierName"].ToString();
                    supplierInfo.ContactDetails = oReader["EmailId"].ToString()+"  " + oReader["ContactNo"].ToString()+" "+ oReader["ZipCode"].ToString();
                    suppliers.Add(supplierInfo);
                }
                sqlConnection.Close();
            }

            foreach (var supplier in suppliers)
            {
                foreach (var supplierScore in supplierScores)
                {
                    if (supplierScore.SupplierId == supplier.SupplierId)
                    {
                        supplier.Score = supplierScore.score;
                    }

                }
            }
            return suppliers;
        }





        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

 
    }
}
