using DiscountCode.Model;
using Genbyte.Base.CoreLib;
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

namespace DiscountCode
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DiscountCodeController : ControllerBase
    {
        public DiscountCodeController()
        {
        }

        /// <summary>
        /// Lấy thông tin mã giảm giá --> check tồn
        /// </summary>
        /// <param name="ma_gg">Mã giảm giá</param>
        /// <param name="ma_dvcs">Danh sách mã vật tư</param>
        /// <returns></returns>
        [HttpGet("get_discount_code_info")]
        #region get_payment_debit
        public IActionResult GetDiscountCodeInfo(string ma_gg)
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
                if (!_service.IsSQLInjectionValid(ma_gg))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //Lấy thông tin mã giảm giá --> check tồn
                DiscountCodeModel code = _service.GetDiscountCodeInfo(ma_gg);
                if (code != null && code.ma_gg != "")
                {
                    model.success = true;
                    model.result = code;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- DiscountCodeController/GetDiscountCodeInfo?ma_gg={ma_gg}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy thông tin chương trình giảm giá
        /// </summary>
        /// <param name="ma_dvcs">Danh sách mã vật tư</param>
        /// <returns></returns>
        [HttpPost("get_discount_program")]
        #region get_payment_debit
        public IActionResult GetDiscountProgram([FromBody] List<string> list_item)
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
                if (!_service.IsSQLInjectionValid(string.Join(",", list_item)))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //Lấy thông tin mã giảm giá --> check tồn
                List<DiscountProgramModel> code = _service.GetDiscountProgram(list_item);
                EntityCollection<DiscountProgramModel> entity = new EntityCollection<DiscountProgramModel>
                {
                    Items = code,
                    PageCount = 1,
                    PageIndex = 0, 
                    PageSize = code.Count,
                    RecordCount = code.Count
                };
                if (code != null)
                {
                    model.success = true;
                    model.result = entity;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetDiscountProgram?list_item={list_item}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
