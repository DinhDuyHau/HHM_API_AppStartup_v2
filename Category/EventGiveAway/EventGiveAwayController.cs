using EventGiveAway;
using EventGiveAway.Model;
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
    public class EventGiveAwayController : ControllerBase
    {
        public EventGiveAwayController()
        {
        }

        /// <summary>
        /// Lấy danh mục sự kiện tặng hàng
        /// </summary>
        /// <param name="ma_cuahang">Mã cửa hàng</param>
        /// <param name="ma_sukien">Mã sự kiện</param>
        /// <param name="ten_sukien">Tên sự kiện</param>
        /// <param name="pageIndex">trang</param>
        /// <param name="pageSize">Số bản ghi một trang</param>
        /// <returns></returns>
        [HttpGet("get_event_giveaway")]
        #region get_event_giveaway
        public IActionResult GetEventGiveAway(string ma_cuahang, string? ma_sukien = "", string? ten_sukien ="", int? pageIndex = 1, int? pageSize = 20)
        {
            if (ma_sukien == null) ma_sukien = "";
            if (ten_sukien == null) ten_sukien = "";
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
                if (!_service.IsSQLInjectionValid(ma_cuahang) || !_service.IsSQLInjectionValid(ma_sukien) || !_service.IsSQLInjectionValid(ten_sukien))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var res = _service.GetEventGiveAway(ma_cuahang, ma_sukien, ten_sukien, (int) pageIndex, (int)pageSize);
                res.Items.ForEach((x) => x.ma_sukien = x.ma_sukien.Trim());
                if (res != null)
                {
                    model.success = true;
                    model.result = res;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EventGiveAwayController/GetEventGiveAway?ma_cuahang={ma_cuahang}&ma_sukien={ma_sukien}&ten_sukien={ten_sukien}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy mã danh mục sự kiện tặng hàng
        /// </summary>
        /// <param name="ma_sukien">Mã sự kiện</param>
        /// <returns></returns>
        [HttpGet("getbyid")]
        #region get_event_giveaway
        public IActionResult GetEventGiveAwayById(string ma_sukien)
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
                if (!_service.IsSQLInjectionValid(ma_sukien))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var res = _service.GetEventGiveAwayById(ma_sukien);
                res.ma_sukien = res.ma_sukien.Trim();
                if (res != null)
                {
                    model.success = true;
                    model.result = res;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EventGiveAwayController/GetEventGiveAwayById?ma_sukien={ma_sukien}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion


        /// <summary>
        /// Lấy chi tiết sự kiện tặng hàng
        /// </summary>
        /// <param name="ma_sukien">Mã sự kiện</param>
        /// <returns></returns>
        [HttpGet("get_event_giveaway_detail")]
        #region get_event_giveaway
        public IActionResult GetEventGiveAwayDetail(string ma_sukien)
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
                if (!_service.IsSQLInjectionValid(ma_sukien))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                var res = _service.GetEventGiveAwayDetail(ma_sukien);
                res.ForEach(x => x.ma_sukien = x.ma_sukien.Trim());
                if (res != null)
                {
                    model.success = true;
                    model.result = res;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EventGiveAwayController/GetEventGiveAwayDetail?ma_sukien={ma_sukien}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
