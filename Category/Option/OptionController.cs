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
        #region get_payment_debit
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
    }
}
