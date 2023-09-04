using Customer.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
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
        public CustomerController()
        {
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

    }
}
