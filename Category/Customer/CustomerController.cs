using Customer.Model;
using Genbyte.Base.CoreLib;
using Genbyte.Base.Security;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly Security _security;
        private readonly IConfiguration _configuration;
        public CustomerController(IOptions<Security> security, IConfiguration configuration)
        {
            this._security = security.Value;
            this._configuration = configuration;
        }

        /// <summary>
        /// Lấy công nợ cho khách hàng
        /// </summary>
        /// <param name="ma_kh">Khách hàng cần lấy công nợ</param>
        /// <param name="ma_dvcs">mã đơn vị</param>
        /// <param name="ngay_ct">Ngày cần lấy thông tin</param>
        /// <returns></returns>
        [HttpGet("get_payment_debit")]
        #region get_payment_debit
        public IActionResult GetPaymentDebit(string ma_kh, string ma_dvcs, DateTime ngay_ct)
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
                if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_dvcs))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                List<PaymentDebtModel> paymentDebts = _service.GetPaymentDebit(ma_kh, ma_dvcs, ngay_ct).Where(x => x.cl_nt > 0).OrderBy(x=>x.ngay_ct).ToList();
                paymentDebts.ForEach(item => item.stt_rec = APIService.EncryptForWebApp(item.stt_rec, this._security.KeyAES, this._security.IVAES));
                if (paymentDebts != null)
                {
                    model.success = true;
                    model.result = paymentDebts;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetServicePrice?ma_kh={ma_kh}&ma_dvcs={ma_dvcs}&ngay_ct={ngay_ct}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion


        ///// <summary>
        ///// Lấy những khoản khách hàng còn cọc lại tại cửa hàng theo chương trình
        ///// </summary>
        ///// <param name="ma_kh">Khách hàng cần lấy cọc</param>
        ///// <param name="ma_dvcs">mã đơn vị</param>
        ///// <param name="ngay_ct">Ngày cần lấy thông tin</param>
        ///// <param name="ma_ctr">Chương trình cần lọc</param>
        ///// <param name="ma_vt">Vật tư cần lọc</param>
        ///// <returns></returns>
        //[HttpGet("get_payment_deposit")]
        //#region get_payment_deposit
        //public IActionResult GetPaymentDeposit(string ma_kh, string ma_dvcs, DateTime ngay_ct, string? ma_ctr, string? ma_vt)
        //{
        //    try
        //    {
        //        CommonObjectModel model = new CommonObjectModel()
        //        {
        //            success = false,
        //            message = "",
        //            result = null
        //        };

          
        //        Service _service = new Service();

        //        //check injection
        //        if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_dvcs))
        //            return BadRequest(new { message = ApiReponseMessage.Error_InputData });

        //        //lấy công nợ
        //        List<PaymentDepositModel> paymentDeposit = _service.GetPaymentDeposit(ma_kh, ma_dvcs, ma_ctr == null ?  "": ma_ctr, ma_vt == null ? "" : ma_vt, ngay_ct)
        //            .Where(x => x.cl_nt > 0).GroupBy(x => x.ma_ctr).Select(
        //             group => new PaymentDepositModel
        //             {
        //                   ma_ctr = group.Key,
        //                   ma_sp = "",
        //                   stt_rec = group.Max(x=>x.stt_rec),
        //                   so_ct = group.Max(x=>x.so_ct),
        //                   ngay_ct = group.Max(x=>x.ngay_ct),
        //                   t_tt_nt = group.Sum(x=>x.t_tt_nt),
        //                   da_tt_nt = group.Sum(x=>x.da_tt_nt),
        //                   cl_nt = group.Sum(x=>x.cl_nt),
        //                   ma_nt = group.Max(x=>x.ma_nt),
        //                   dien_giai = group.Max(x=>x.dien_giai),
        //             }).OrderBy(x => x.ngay_ct).ToList();

        //        if (paymentDeposit != null)
        //        {
        //            EntityCollection<PaymentDepositModel> entity = new EntityCollection<PaymentDepositModel>
        //            {
        //                PageIndex = 1,
        //                PageSize = paymentDeposit.Count(),
        //                PageCount = 1,
        //                RecordCount = paymentDeposit.Count(),
        //                Items = paymentDeposit
        //            };
        //            model.success = true;
        //            model.result = entity;
        //        }

        //        return Ok(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Insert(Startup.Unit, $"GET -- CustomerController/GetPaymentDeposit?ma_kh={ma_kh}&ma_dvcs={ma_dvcs}&ma_ctr={ma_ctr}&ma_vt={ma_vt}&ngay_ct={ngay_ct}", ex);
        //        return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
        //    }
        //}
        //#endregion
        /// <summary>
        /// Lấy những khoản khách hàng còn cọc lại tại cửa hàng 
        /// </summary>
        /// <param name="ma_kh">Khách hàng cần lấy cọc</param>
        /// <param name="ma_dvcs">mã đơn vị</param>
        /// <param name="ngay_ct">Ngày cần lấy thông tin</param>
        /// <param name="ma_ctr">Chương trình cần lọc</param>
        /// <param name="ma_vt">Vật tư cần lọc</param>
        /// <returns></returns>
        [HttpGet("get_payment_deposit")]
        #region get_payment_deposit
        public IActionResult GetPaymentDepositDetail(string ma_kh, string ma_dvcs, DateTime ngay_ct, string? ma_ctr, string? ten_ctr, string? ma_vt, string? ten_vt)
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
                if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_dvcs) || !_service.IsSQLInjectionValid(ma_ctr) || !_service.IsSQLInjectionValid(ten_ctr) || !_service.IsSQLInjectionValid(ma_vt) || !_service.IsSQLInjectionValid(ten_vt))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                List<PaymentDepositModel> paymentDeposit = 
                    _service.GetPaymentDeposit(ma_kh, ma_dvcs, ma_ctr == null ? "" : ma_ctr, ten_ctr == null ? "" : ten_ctr, ma_vt == null ? "" : ma_vt, ten_vt == null ? "" : ten_vt, ngay_ct).ToList();
                paymentDeposit.ForEach(item => item.stt_rec = APIService.EncryptForWebApp(item.stt_rec, this._security.KeyAES, this._security.IVAES));
                if (paymentDeposit != null)
                {
                    EntityCollection<PaymentDepositModel> entity = new EntityCollection<PaymentDepositModel>
                    {
                        PageIndex = 1,
                        PageSize = paymentDeposit.Count(),
                        PageCount = 1,
                        RecordCount = paymentDeposit.Count(),
                        Items = paymentDeposit
                    };
                    model.success = true;
                    model.result = entity;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- CustomerController/GetPaymentDeposit?ma_kh={ma_kh}&ma_dvcs={ma_dvcs}&ma_ctr={ma_ctr}&ma_vt={ma_vt}&ngay_ct={ngay_ct}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy tổng số điểm quy đổi hiện có của khách hàng dựa theo ngày chứng từ
        /// </summary>
        [HttpGet("get_conversion_point")]
        #region get_payment_debit
        public IActionResult GetConversionPoint(string ma_kh, DateTime ngay_ct)
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
                if (!_service.IsSQLInjectionValid(ma_kh))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy điểm
                decimal diem_qd = _service.GetConversionPoint(ma_kh, ngay_ct);
                if (diem_qd != null)
                {
                    model.success = true;
                    model.result = diem_qd;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- CustomerController/GetConversionPoint?ma_kh={ma_kh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Lấy thông tin khách hàng theo mã số thuế
        /// </summary>
        [HttpGet("get_infomation_by_tax")]
        #region get_payment_debit
        public IActionResult GetCustomerInfoByTax(string ma_so_thue)
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
                if (!_service.IsSQLInjectionValid(ma_so_thue))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                string serviceTax = _configuration["Customer:TaxService"];

                CustomerInfoByTax customer_info = _service.GetCustomerInfoByTax(serviceTax, ma_so_thue);
                
                if (customer_info != null)
                {
                    if (customer_info.error)
                    {
                        model.success = false;
                        model.message = "not_found_customer_by_tax";
                    }
                    else
                    {
                        model.success = true;
                        model.result = customer_info;
                    }
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- CustomerController/GetCustomerInfoByTax?tax={ma_so_thue}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpPost("phonecheck/{phone}")]
        #region PhoneCheck
        public async Task<dynamic> PhoneCheck(string phone)
        {
            try
            {
                var response = await Service.PhoneCheck(phone);
                var content = await response.Content.ReadAsStringAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- CustomerController/PhoneCheck", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
