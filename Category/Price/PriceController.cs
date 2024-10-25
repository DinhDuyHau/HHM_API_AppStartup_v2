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
using Price.Model;
using System.Runtime.InteropServices;

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
        public IActionResult GetServicePrice(string ma_dichvu, string ma_cuahang, string? ma_vt = "", decimal? gia_ban = 0)
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
                ServicePriceModel price_item = _service.GetPriceOfServiceItem(ma_vt ?? "", ma_dichvu, ma_cuahang, gia_ban ?? 0);
                if (price_item != null)
                {
                    model.success = true;
                    model.result = price_item;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetServicePrice?ma_vt={ma_vt}&ma_dichvu={ma_dichvu}&ma_cuhang={ma_cuahang}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion


        /// <summary>
        /// Lấy giá bán của hàng thu cũ 
        /// </summary>
        /// <param name="ma_vt">mã hàng cần lấy thông tin</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_renew_price")]
        #region GetRenewPrice
        public IActionResult GetRenewPrice(string ma_vt, string ma_cuahang, string? ma_ncc = "")
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
                if (!_service.IsSQLInjectionValid(ma_vt) || !_service.IsSQLInjectionValid(ma_cuahang) || !_service.IsSQLInjectionValid(ma_ncc))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy giá dịch vụ
                List<RenewPriceModel> price_item = _service.GetRenewPrice(ma_vt, ma_cuahang, ma_ncc);
                if (price_item != null)
                {
                    model.success = true;
                    model.result = price_item;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetRenewPrice?ma_imei={ma_vt}&ma_cuahang={ma_cuahang}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Check giá điều chỉnh và lấy tiền hỗ trợ từ vật tư thu cũ
        /// </summary>
        [HttpGet("renew_adjust_buy_price")]
        #region GetRenewAdjustBuyPrice
        public IActionResult GetRenewAdjustBuyPrice(DateTime ngay_ct, string? ma_cttc, string ma_ncc, string loai_hang_mua, string ma_vt_mua, string ma_vt_ban, decimal gia_ban, decimal gia_dc, string ma_td3)
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
                if (!_service.IsSQLInjectionValid(ma_cttc) || !_service.IsSQLInjectionValid(ma_ncc) || !_service.IsSQLInjectionValid(loai_hang_mua) 
                    || !_service.IsSQLInjectionValid(ma_vt_mua) || !_service.IsSQLInjectionValid(ma_vt_ban))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy giá dịch vụ
                List<AdjustPriceModel> entity = _service.GetRenewAdjustBuyPrice(ngay_ct, ma_cttc, ma_ncc, loai_hang_mua, ma_vt_mua, ma_vt_ban, gia_ban, gia_dc, ma_td3);
                if (entity != null)
                {
                    model.success = true;
                    model.result = entity;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetRenewAdjustBuyPrice", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy giá bán của hàng bán trả lại
        /// </summary>
        /// <param name="ma_vt">mã hàng cần lấy thông tin</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <returns></returns>
        [HttpGet("get_return_price")]
        #region GetReturnPrice
        public IActionResult GetReturnPrice(string ma_vt, string ma_imei, string ma_cuahang, DateTime ngay_ct, decimal gia_nt, decimal? rate = null, decimal? tien_giam = 0)
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
                if (!_service.IsSQLInjectionValid(ma_vt) || !_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy giá dịch vụ
                ReturnPriceModel price_item = _service.GetReturnPrice(ma_vt, ma_imei, ma_cuahang, ngay_ct, gia_nt, rate ?? -1, tien_giam ?? 0);
                if (price_item != null)
                {
                    model.success = true;
                    model.result = price_item;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- PriceController/GetReturnPrice?ma_imei={ma_vt}&ma_cuahang={ma_cuahang}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
