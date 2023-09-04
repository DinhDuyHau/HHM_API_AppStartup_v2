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

namespace Price
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PriceController : ControllerBase
    {
        public PriceController() 
        { 
        }

        /// <summary>
        /// Lấy giá bán của dịch vụ
        /// </summary>
        /// <param name="ma_dichvu">mã dịch vụ cần lấy thông tin</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("getserviceprice")]
        #region GetImeiInStore
        public IActionResult GetServicePrice(string ma_dichvu, string ma_cuahang)
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
                if(!_service.IsSQLInjectionValid(ma_dichvu) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy giá dịch vụ
                ServicePriceModel price_item = _service.GetPriceOfServiceItem(ma_dichvu, ma_cuahang);
                if (price_item != null)
                {
                    model.success = true;
                    model.result = price_item;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetServicePrice?ma_imei={ma_dichvu}&ma_cuhang={ma_cuahang}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        

    }
}
