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
                Logger.Insert(Startup.Unit, $"HttpGet -- InvPaymentController/GetRefCode", ex);
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
                Logger.Insert(Startup.Unit, $"HttpGet -- InvPaymentController/GetRefCode", ex);
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
                Logger.Insert(Startup.Unit, $"HttpGet -- InvPaymentController/GetRefCode", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
            return Ok(model);
        }

    }
}
