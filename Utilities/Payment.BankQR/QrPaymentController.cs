using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using System.Collections;
using Genbyte.Base.CoreLib;
using System.Text.RegularExpressions;
using System.Data;
using Genbyte.Base.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SqlClient;
using Payment.BankQR.Models;
using System.Text.Json;

namespace Payment.BankQR
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class QrPaymentController : ControllerBase
    {
        private readonly IMemoryCache memory_cache;
        private readonly IConfiguration _configuration;
        private readonly Security _security;
        private readonly IHttpService _httpService;

        // version của API Payment
        private const string API_VERSION = "v1";

        // danh sách mã của các ngân hàng đã thực hiện kết nối thanh toán QR
        // mã bank phải khớp với khai báo trong bảng parner_userinfo (db:SYS)
        private readonly string[] banks = { "MB" };

        public QrPaymentController(IMemoryCache memoryCache, IConfiguration configuration, IOptions<Security> security, IHttpService httpService)
        {
            this.memory_cache = memoryCache;
            _configuration = configuration;
            _security = security.Value;
            _httpService = httpService;
        }

        [HttpGet("getrefcode/{bank_code}")]
        public async Task<IActionResult> GetRefCode(string bank_code)
        {
            PaymentObjectModel? model = new PaymentObjectModel()
            {
                error_code = 0,
                success = false,
                message = "",
                result = null
            };

            if (!this.banks.Contains(bank_code.ToUpper()))
            {
                model.message = $"Không tồn tại thanh toán của ngân hàng {bank_code}";
                return Ok(model);
            }

            try
            {
                string api_url = this._configuration[$"QrPayment:{bank_code}"];
                string url = api_url + (api_url.EndsWith("/") ? "" : "/") + $"{API_VERSION}/Payment/{bank_code}/getRefCode";

                PaymentService service = new PaymentService();
                string auth_code = service.GetParnerIdByName(this.memory_cache, bank_code);

                _httpService.RemoveDefaultHeader("Authorization");
                _httpService.AddDefaultHeader("Authorization", auth_code);

                model = await _httpService.GetAsync<PaymentObjectModel>(url);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"HttpGet -- QrPaymentController/GetRefCode", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
            return Ok(model);
        }

        [HttpPost("createQR/{bank_code}")]
        public async Task<IActionResult> CreateQR(string bank_code)
        {
            PaymentObjectModel? model = new PaymentObjectModel()
            {
                error_code = 0,
                success = false,
                message = "",
                result = null
            };

            if (!this.banks.Contains(bank_code.ToUpper()))
            {
                model.message = $"Không tồn tại thanh toán của ngân hàng {bank_code}";
                return Ok(model);
            }

            try
            {
                string api_url = this._configuration[$"QrPayment:{bank_code}"];
                string url = api_url + (api_url.EndsWith("/") ? "" : "/") + $"{API_VERSION}/Payment/{bank_code}/createqr";

                PaymentService service = new PaymentService();
                string auth_code = service.GetParnerIdByName(this.memory_cache, bank_code);

                _httpService.RemoveDefaultHeader("Authorization");
                _httpService.AddDefaultHeader("Authorization", auth_code);

                string rq_body = await new StreamReader(Request.Body).ReadToEndAsync();

                model = await _httpService.PostAsync<PaymentObjectModel>(url, rq_body);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"HttpPost -- QrPaymentController/CreateQR", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
            return Ok(model);
        }

        [HttpDelete("deleteQR/{bank_code}")]
        public async Task<IActionResult> DeleteQR(string bank_code)
        {
            PaymentObjectModel? model = new PaymentObjectModel()
            {
                error_code = 0,
                success = false,
                message = "",
                result = null
            };

            if (!this.banks.Contains(bank_code.ToUpper()))
            {
                model.message = $"Không tồn tại thanh toán của ngân hàng {bank_code}";
                return Ok(model);
            }

            try
            {
                string api_url = this._configuration[$"QrPayment:{bank_code}"];
                string url = api_url + (api_url.EndsWith("/") ? "" : "/") + $"{API_VERSION}/Payment/{bank_code}/deleteqr";

                PaymentService service = new PaymentService();
                string auth_code = service.GetParnerIdByName(this.memory_cache, bank_code);

                _httpService.RemoveDefaultHeader("Authorization");
                _httpService.AddDefaultHeader("Authorization", auth_code);

                string rq_body = await new StreamReader(Request.Body).ReadToEndAsync();

                model = await _httpService.DeleteAsync<PaymentObjectModel>(url, rq_body);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"HttpDelete -- QrPaymentController/DeleteQR", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
            return Ok(model);
        }


        [HttpPost("getftcodebycustomer")]
        public async Task<IActionResult> GetFTCodeByCustomer([FromBody] FTCodeReqModel body, [FromQuery] int page_index = 1, [FromQuery] int page_size = 0)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };

            string ma_kh = body.ma_kh;
            CoreService service = new CoreService();

            //Lấy dữ liệu theo shop đăng nhập hiện tại
            string ma_cuahang = Startup.Shop;

            //check sql injection
            if(!service.IsSQLInjectionValid(ma_kh))
            {
                return BadRequest(new { message = ApiReponseMessage.Error_InputData });
            }
            if(body.filter != null && body.filter.Count > 0) foreach (Dictionary<string, string> dic in body.filter)
                {
                    string item_value = dic.Values.Last();
                    if (!service.IsSQLInjectionValid(item_value))
                        return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                }

            try
            {
                //convert filter to json
                string filter_json = "";
                if(body.filter != null && body.filter.Count > 0)
                    filter_json = JsonSerializer.Serialize(body.filter, new JsonSerializerOptions { WriteIndented = false });

                string sql = "exec Genbyte$Payment$GetFTCodeForRefund @ma_kh, @ma_cuahang, @page_index, @page_size, @extFilter";
                List<SqlParameter> paras = new List<SqlParameter>();
                #region add parameters
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@ma_kh",
                    SqlDbType = SqlDbType.Char,
                    Value = ma_kh.Replace("'", "''")
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@ma_cuahang",
                    SqlDbType = SqlDbType.Char,
                    Value = ma_cuahang
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@page_index",
                    SqlDbType = SqlDbType.Int,
                    Value = page_index
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@page_size",
                    SqlDbType = SqlDbType.Int,
                    Value = page_size
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@extFilter",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = filter_json
                });
                #endregion
                DataSet ds = service.ExecSql2DataSet(sql, paras);
                if(ds != null && ds.Tables.Count >= 2)
                {
                    int record_per_page = page_size;
                    int totalPage = 0;
                    int row_count = 0;
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        totalPage = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalPage"]);
                        record_per_page = Convert.ToInt32(ds.Tables[0].Rows[0]["PageSize"]);
                        row_count = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRecordCount"]);
                    }

                    EntityCollection<FTCodeResponse> entities = new EntityCollection<FTCodeResponse>();
                    entities.PageSize = record_per_page;
                    entities.PageIndex = page_index;
                    entities.PageCount = totalPage;
                    entities.RecordCount = row_count;
                    entities.Items = new List<FTCodeResponse>();

                    if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {
                        entities.Items = ds.Tables[1].ToList<FTCodeResponse>().ToList();
                    }

                    model.success = true;
                    model.result = entities;
                }
            }
            catch(Exception ex)
            {
                Logger.Insert(Startup.Unit, $"HttpPost -- QrPaymentController/GetFTCodeByCustomer", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
            return Ok(model);
        }
    }
}
