using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Genbyte.Base.CoreLib;
using System.Data;
using Service.Model;
using Mail.Model;
using Mail;
using Microsoft.Extensions.Options;
using Genbyte.Base.Security;
using System.Data.SqlClient;

namespace Servive
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly SmtpConfig _smtpConfig;
        private readonly Security _security;
        public ServiceController(IOptions<SmtpConfig> smtpConfig, IOptions<Security> security)
        {
            _smtpConfig = smtpConfig.Value;
            _security = security.Value;
        }

        /// <summary>
        /// Lấy giá bán của dịch vụ
        /// </summary>
        /// <param name="ma_dichvu">mã dịch vụ cần lấy thông tin</param>
        /// <param name="ma_dichvu">số lượng</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_key_service")]
        #region GetKeyService
        public IActionResult GetKeyService(string ma_dichvu, decimal so_luong)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                //check injection
                if(!_service.IsSQLInjectionValid(ma_dichvu))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                List<KeyServiceModel> price_item = _service.GetKeyService(ma_dichvu, so_luong);
                if (price_item != null)
                {
                    model.success = true;
                    model.result = price_item.Count == so_luong;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetServicePrice?ma_imei={ma_dichvu}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Gửi email
        /// </summary>
        [HttpPost("send_email")]
        #region send_email
        public IActionResult SendEmail(string stt_rec)
        {
            CommonObjectModel model = new CommonObjectModel
            {
                message = "send_email_error",
                result = null,
                success = false,
            };
            stt_rec = APIService.DecryptForWebApp(stt_rec, _security.KeyAES, _security.IVAES);
            Service _service = new Service();

            //check injection
            if (!_service.IsSQLInjectionValid(stt_rec))
                return BadRequest(new { message = ApiReponseMessage.Error_InputData });

            // check xem đã gửi hay chưa, nếu rồi ko gửi lại nữa
            if(_service.isEmailSent(stt_rec))
            {
                model.message = "";
                return Ok(model);
            }

            List<KeyToSendEmail> keyToSends = _service.GetKeyToSendEmail(stt_rec);
            if(keyToSends == null || keyToSends.Count == 0)
            {
                model.message = "";
                return Ok(model);
            }
            string title = "Hoàng Hà Mobile";
            string body = _service.GenerateHTML(keyToSends);

            MailRequest request = new MailRequest
            {
                title = title,
                body = body,
                send_to = keyToSends[0].email0
            };
            MailService mailService = new MailService();
            try
            {
                mailService.Send(request, _smtpConfig);
            }
            catch (Exception ex)
            {
                return Ok(model);
            }

            // update count_sent tăng lên khi gửi thành công
            _service.UpdateCountSent(stt_rec);

            model.success = true;
            model.message = "send_email_success";
            return Ok(model);
        }
        #endregion
        /// <summary>
        /// Lấy danh sách hoá đơn của dịch vụ
        /// </summary>
        /// <param name="ma_dichvu">mã dịch vụ cần lấy thông tin</param>
        /// <param name="ma_dichvu">số lượng</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_sold_service_order")]
        #region GetSoldServiceOrder
        public IActionResult GetSoldServiceOrder(string so_ct, string ma_cuahang)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                //check injection
                if (!_service.IsSQLInjectionValid(so_ct) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var service = _service.GetSoldServiceOrder(so_ct, ma_cuahang?? Startup.Shop);
                if (service != null)
                {
                    model.success = true;
                    model.result = service;
                }
                
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ServiceController/GetSoldServiceOrder?so_ct={so_ct}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        
        /// <summary>
        /// Lấy danh sách hoá đơn của dịch vụ
        /// </summary>
        /// <param name="ma_dichvu">mã dịch vụ cần lấy thông tin</param>
        /// <param name="ma_dichvu">số lượng</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_sold_service_orders")]
        #region GetSoldServiceOrder
        public IActionResult GetSoldServiceOrders(string ma_kh, string ma_cuahang)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                //check injection
                if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var service = _service.GetSoldServiceOrders(ma_kh, ma_cuahang ?? Startup.Shop);
                if (service != null)
                {
                    model.success = true;
                    model.result = service;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ServiceController/GetSoldServiceOrders?ma_kh={ma_kh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy danh sách phiếu dịch vụ đủ điều kiện trả lại
        /// </summary>
        /// <param name="ma_kh">mã khách hàng</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_orders_service_return")]
        #region GetSoldServiceOrder
        public IActionResult GetOrdersServiceReturn(string ma_kh, string ma_cuahang)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                //check injection
                if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var service = _service.GetOrdersServiceReturn(ma_kh, ma_cuahang ?? Startup.Shop);
                if (service != null)
                {
                    model.success = true;
                    model.result = service;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ServiceController/GetOrdersServiceReturn?ma_kh={ma_kh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy danh sách phiếu dịch vụ đủ điều kiện mua lại
        /// </summary>
        /// <param name="ma_kh">mã khách hàng</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_orders_buyback_service")]
        #region GetSoldServiceOrder
        public IActionResult GetOrdersBuyBackService(string ma_kh, string ma_cuahang)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                //check injection
                if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var service = _service.GetOrdersBuyBackService(ma_kh, ma_cuahang ?? Startup.Shop);
                if (service != null)
                {
                    model.success = true;
                    model.result = service;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ServiceController/GetOrdersBuyBackService?ma_kh={ma_kh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy phiên bản ứng dụng FE được lưu trong options
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get_version_app")]
        #region GetVersionApp
        public IActionResult GetVersionApp()
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                var service = _service.GetVersionApp();
                if (service != null)
                {
                    model.success = true;
                    model.result = service;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ServiceController/GetVersionApp", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
