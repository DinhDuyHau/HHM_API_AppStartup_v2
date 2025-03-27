using Option.Model;
using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Option
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class OptionController : ControllerBase
    {
        public OptionController()
        {
        }

        /// <summary>
        /// Lấy hệ số quy đổi điểm
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_point_rate_exchange")]
        #region get_point_rate_exchange
        public IActionResult GetPointConversionFactor()
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
                //lấy option
                OptionModel option = _service.GetOptionByName("m_point_rate_exchange");

                if (option != null)
                {
                    option.val = int.Parse(option.val.ToString());
                    model.success = true;
                    model.result = option;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- OptionController/GetPointConversionFactor/m_point_rate_exchange", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Lấy hệ số quy đổi từ điểm sang tiền
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_rate_exchange_reverse")]
        #region m_exchange_reverse
        public IActionResult GetPointConversionFactorReverse()
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
                //lấy option
                OptionModel option = _service.GetOptionByName("m_exchange_reverse");

                if (option != null)
                {
                    option.val = int.Parse(option.val.ToString());
                    model.success = true;
                    model.result = option;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- OptionController/GetPointConversionFactor/m_point_rate_exchange", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        
        /// <summary>
        /// Lấy hệ số quy đổi điểm
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_option/{name}")]
        #region get_payment_debit
        public IActionResult GetPointConversionFactor(string name)
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
                if (!_service.IsSQLInjectionValid(name))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                OptionModel option = _service.GetOptionByName(name);

                if (option != null)
                {
                    model.success = true;
                    model.result = option;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- OptionController/GetPointConversionFactor/"+ name, ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy hệ số phần bán hàng
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_option")]
        #region get_payment_debit
        public IActionResult GetSaleOptions()
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

                List<OptionModel> option = _service.GetOptionBySubSystem("BH");
                Dictionary<string, object> options = new Dictionary<string, object>();
                option.ForEach(option => {
                    options[option.name] = option.val;
                });

                if (option != null)
                {
                    model.success = true;
                    model.result = options;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- OptionController/GetSaleOptions", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// Lấy ngày chứng từ
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("getdate")]
        public IActionResult getDate()
        {
            try
            {
                var currentDate = DateTime.Now;
                //var testDate = new DateTime(2025, 3, 27, 20, 0, 0);

                CommonObjectModel model = new CommonObjectModel()
                {
                    success = currentDate != default(DateTime),
                    message = currentDate != default(DateTime) ? "" : "Runtime_err",
                    result = currentDate
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Shop, $"GET -- OptionController/getDate", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
    }
}
