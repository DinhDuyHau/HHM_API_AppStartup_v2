using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Genbyte.Sys.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Genbyte.Base.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace EInvoice
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EInvoiceController: ControllerBase
    {

        private readonly Security _security;
        private readonly IConfiguration _configuration;
        public EInvoiceController(IConfiguration configuration, IOptions<Security> security)
        {
            this._configuration = configuration;
            _security = security.Value;
        }

        /// <summary>
        /// Lấy file PDF từ hóa đơn điện tử
        /// </summary>
        /// <param name="voucherInfo">Thông tin chứng từ</param>
        /// <returns></returns>
        [HttpPost("invoicePDF")]
        #region GetPDFInvoice
        public IActionResult GetInvoicePDF(VoucherEntity voucherInfo)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service(this._configuration);
                string res = _service.GetInvoicePDF(voucherInfo).Result.ToString();
                //Response response = JsonConvert.DeserializeObject<Response>(res);
                var result = JsonConvert.DeserializeObject<Response>(res);
                model.result = result.d;
                if (model.result != null)
                    model.success = true;
                if(result.d != null && result.d.fileToBytes == null)
                {
                    model.success = false;
                    model.message = "cannot_view_pdf_invoice";
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- EInvoiceController/invoicePDF", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpPost("invoicePDFV2")]
        #region GetPDFInvoiceV2
        public IActionResult GetInvoicePDFV2(string stt_rec, string ma_ct, string type = "official")
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service(this._configuration);
                stt_rec = APIService.DecryptForWebApp(stt_rec, _security.KeyAES, _security.IVAES);
                string res = _service.GetInvoicePDF(stt_rec, ma_ct, type).Result.ToString();
                //Response response = JsonConvert.DeserializeObject<Response>(res);
                var result = JsonConvert.DeserializeObject<Response>(res);
                model.result = result.d;
                if (model.result != null)
                    model.success = true;
                if (result.d != null && result.d.fileToBytes == null)
                {
                    model.success = false;
                    model.message = "cannot_view_pdf_invoice";
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- EInvoiceController/invoicePDF", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lập nháp hóa đơn
        /// </summary>
        /// <param name="voucherInfo">Thông tin chứng từ</param>
        /// <returns></returns>
        [HttpPost("CreateDraft")]
        #region CreateDraft
        public IActionResult CreateDraft(VoucherEntity voucherInfo)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service(this._configuration);
                string res = _service.CreateDraft(voucherInfo).Result.ToString();
                //Response response = JsonConvert.DeserializeObject<Response>(res);
                var result = JsonConvert.DeserializeObject<Response>(res);
                model.result = result.d;
                if (model.result != null)
                {
                    if(result.d.voucherId == voucherInfo.stt_rec)
                    {
                        model.success = true;
                        model.message = "create_draft_invoice_success";
                    }
                    else
                    {
                        model.message = "cannot_create_draft_invoice";
                    }
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- EInvoiceController/CreateDraft", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lập nháp hóa đơn
        /// </summary>
        /// <param name="voucherInfo">Thông tin chứng từ</param>
        /// <returns></returns>
        [HttpPost("CreateDraftV2")]
        #region CreateDraft
        public IActionResult CreateDraftV2(string stt_rec, string ma_ct)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service(this._configuration);
                stt_rec = APIService.DecryptForWebApp(stt_rec, _security.KeyAES, _security.IVAES);

                //Kiểm tra trạng thái chứng từ "hoàn thành" trước khi lập hóa đơn
                //if(!_service.CheckValidVoucherStatus(stt_rec, ma_ct))
                //{
                //    model.success = false;
                //    model.message = "einvoice_status_invalid";
                //    return Ok(model);
                //}

                string res = _service.CreateDraft(stt_rec, ma_ct).Result.ToString();
                //Response response = JsonConvert.DeserializeObject<Response>(res);
                var result = JsonConvert.DeserializeObject<Response>(res);
                model.result = result.d;
                if (model.result != null)
                {
                    if (string.IsNullOrEmpty(result.d.description) && string.IsNullOrEmpty(result.d.errorCode))
                    {
                        model.success = true;
                        model.message = "create_draft_invoice_success";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(result.d.description))
                        {
                            model.message = result.d.description;
                        }
                        else
                        {
                            model.message = "cannot_create_draft_invoice";
                        }
                    }
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- EInvoiceController/CreateDraft", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy thông tin phát hành
        /// </summary>
        /// <param name="voucherInfo">Thông tin chứng từ</param>
        /// <returns></returns>
        [HttpPost("GetPublishedInv")]
        #region GetPublishedInv
        public IActionResult GetPublishedInv(VoucherEntity voucherInfo)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service(this._configuration);
                string res = _service.GetPublishedInv(voucherInfo).Result.ToString();
                //Response response = JsonConvert.DeserializeObject<Response>(res);
                var result = JsonConvert.DeserializeObject<Response>(res);
                model.result = result.d;
                if (model.result != null)
                {
                    if (result.d.voucherId == voucherInfo.stt_rec)
                    {
                        model.success = true;
                        model.message = "get_published_invoice_success";
                    }
                    else
                    {
                        model.message = "cannot_get_published_invoice";
                    }
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- EInvoiceController/CreateDraft", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        [HttpPost("GetPublishedInvV2")]
        #region GetPublishedInvV2
        public IActionResult GetPublishedInvV2(string stt_rec, string ma_ct)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service(this._configuration);
                stt_rec = APIService.DecryptForWebApp(stt_rec, _security.KeyAES, _security.IVAES);
                string res = _service.GetPublishedInv(stt_rec, ma_ct).Result.ToString();
                //Response response = JsonConvert.DeserializeObject<Response>(res);
                var result = JsonConvert.DeserializeObject<Response>(res);
                model.result = result.d;
                if (model.result != null)
                {
                    if (result.d.voucherId == stt_rec)
                    {
                        model.success = true;
                        model.message = "get_published_invoice_success";
                    }
                    else
                    {
                        model.message = "cannot_get_published_invoice";
                    }
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- EInvoiceController/CreateDraft", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
