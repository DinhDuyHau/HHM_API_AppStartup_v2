using Genbyte.Component.Voucher;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Microsoft.AspNetCore.Mvc;
using VoucherWebsite.Model;

namespace VoucherWebsite
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VoucherWebsiteController : ControllerBase
    {
        [HttpPost("check")]
        public async Task<dynamic> Check(VoucherPayloadV2 payload)
        {
            try
            {
                var response = await Service.VoucherCheckV2(payload);
                var raw = await response.Content.ReadAsStringAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
                var content = contentType.Contains("json") ? Service.TryFormatJson(raw) : raw;

                // log request
                CommonService.LogVoucherRequest(payload.Voucher, content, HttpContext, payload);

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- VoucherWebsiteController/Check", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
    }
}
