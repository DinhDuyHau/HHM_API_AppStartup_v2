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
using System.Web;
using Imei.Models;
using Microsoft.Extensions.Configuration;

namespace Imei
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ImeiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ImeiController(IConfiguration _configuration) 
        { 
            this._configuration = _configuration;
        }

        /// <summary>
        /// Lấy thông tin imei theo cửa hàng
        /// </summary>
        /// <param name="ma_imei">imei cần lấy thông tin</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <param name="ma_ct">mã chứng từ</param>
        /// <param name="ma_kh">Tên sàn TMĐT với ma_ct = BHC</param>
        /// <returns></returns>
        [HttpGet("getinstore")]
        #region GetImeiInStore
        public IActionResult GetImeiInStore(string ma_imei, string ma_cuahang, string ma_ct, string? ma_kh)
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
                if(!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang)
                    || !_service.IsSQLInjectionValid(ma_ct))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                DataSet ds = _service.GetImeiInStore(ma_imei, ma_cuahang, ma_ct, ma_kh);
                if(ds != null && ds.Tables.Count >= 2)
                {
                    ImeiState imei_state = null;
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        imei_state = ds.Tables[0].ToList<ImeiState>().FirstOrDefault()!;
                    }
                    if(imei_state != null)
                    {
                        model.success = imei_state.exists_yn && imei_state.in_store_yn
                                        && !imei_state.bao_hanh_yn
                                        && !imei_state.dieu_chuyen_yn 
                                        && !imei_state.xuat_yn
                                        && !imei_state.ban_hang_yn
                                        && !imei_state.tra_ncc_yn
                                        && !imei_state.dat_hang_yn;

                        //check vật tư được đánh dấu cho phép xuất bán liên kết
                        if (ma_ct == "BHD")
                            model.success = imei_state.ban_lk_yn;

                        if (!imei_state.exists_yn)
                            model.message = "exists_yn_no";
                        else if (!imei_state.in_store_yn)
                            model.message = "in_store_yn_no";
                        else if (imei_state.bao_hanh_yn)
                            model.message = "imei_in_warranty_state";
                        else if (imei_state.dat_hang_yn)
                            model.message = "dat_hang_yn_yes";
                        else if (imei_state.dieu_chuyen_yn)
                            model.message = "dieu_chuyen_yn_yes";
                        else if (imei_state.xuat_yn)
                            model.message = "xuat_yn_yes";
                        else if (imei_state.ban_hang_yn)
                            model.message = "ban_hang_yn_yes";
                        else if (imei_state.tra_ncc_yn)
                            model.message = "tra_ncc_yn_yes";
                        else if (ma_ct == "BHD" && !imei_state.ban_lk_yn)
                            model.message = "ban_lk_yn_fail";

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            List<Dictionary<string, object>> item_info = Converter.TableToDictionary(ds.Tables[1]);

                            //Hàng khuyến mại tặng kèm
                            List<Dictionary<string, object>> promotion = _service.GetPromotionByImei(ma_cuahang, ma_imei);
                            item_info[0].Add("promotions", promotion);

                            //add danh sách phí sàn đối với chứng từ bán hàng TMĐT (BHC)
                            if((ma_ct == "BHC" || ma_ct == "bhc") 
                                && ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                            {
                                IList<ECommFee> list_fee = ds.Tables[2].ToList<ECommFee>();
                                item_info[0].Add("ecomm_fee", list_fee);
                            }

                            model.result = item_info;
                        }                            
                    }
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/GetImeiInStore?ma_imei={ma_imei}&ma_cuhang={ma_cuahang}&ma_ct={ma_ct}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy thông tin imei theo cửa hàng
        /// </summary>
        /// <param name="ma_imei">imei cần lấy thông tin</param>
        /// <param name="ma_cuahang">mã cửa hàng</param>
        /// <param name="ma_ct">mã chứng từ</param>
        /// <param name="ma_kh">Tên sàn TMĐT với ma_ct = BHC</param>
        /// <returns></returns>
        [HttpPost("get_price_renew")]
        #region GetImeiInStore
        public IActionResult GetPriceRenew(RenewModel renew)
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
                if (!_service.IsSQLInjectionValid(renew.ma_imei) || !_service.IsSQLInjectionValid(renew.ma_cuahang)
                    || !_service.IsSQLInjectionValid(renew.ma_ncc))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                renew.ma_imei = HttpUtility.UrlDecode(renew.ma_imei);
                //lấy trạng thái & thông tin imei
                DataSet ds = _service.GetPriceRenew(renew);
                if (ds != null && ds.Tables.Count >= 2)
                {
                    ImeiState imei_state = null;
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        imei_state = ds.Tables[0].ToList<ImeiState>().FirstOrDefault()!;
                    }
                    if (imei_state != null)
                    {
                        model.success = imei_state.exists_yn && imei_state.in_store_yn
                                        && !imei_state.bao_hanh_yn
                                        && !imei_state.dieu_chuyen_yn
                                        && !imei_state.xuat_yn
                                        & !imei_state.ban_hang_yn
                                        && !imei_state.tra_ncc_yn
                                        && !imei_state.dat_hang_yn;

                        if (!imei_state.exists_yn)
                            model.message = "exists_yn_no";
                        else if (!imei_state.in_store_yn)
                            model.message = "in_store_yn_no";
                        else if (imei_state.bao_hanh_yn)
                            model.message = "imei_in_warranty_state";
                        else if (imei_state.dat_hang_yn)
                            model.message = "dat_hang_yn_yes";
                        else if (imei_state.dieu_chuyen_yn)
                            model.message = "dieu_chuyen_yn_yes";
                        else if (imei_state.xuat_yn)
                            model.message = "xuat_yn_yes";
                        else if (imei_state.ban_hang_yn)
                            model.message = "ban_hang_yn_yes";
                        else if (imei_state.tra_ncc_yn)
                            model.message = "tra_ncc_yn_yes";

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            List<Dictionary<string, object>> item_info = Converter.TableToDictionary(ds.Tables[1]);

                            //Hàng khuyến mại tặng kèm
                            List<Dictionary<string, object>> promotion = _service.GetPromotionByImei(renew.ma_cuahang, renew.ma_imei);
                            item_info[0].Add("promotions", promotion);

                            model.result = item_info;
                        }
                    }
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/GetPriceRenew?ma_imei={renew.ma_imei}&ma_cuhang={renew.ma_cuahang}&ma_ncc={renew.ma_ncc}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy thông tin về tình trạng của imei
        /// </summary>
        /// <param name="imeis">danh sách imei cần truy vấn</param>
        /// <returns></returns>
        [HttpPost("getstate")]
        #region GetImeiState
        public IActionResult GetImeiState([FromBody] List<string> imeis)
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
                if (!_service.IsSQLInjectionValid(imeis.ToArray()))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                imeis.ForEach(x => x = HttpUtility.UrlDecode(x));
                for (int i = 0; i < imeis.Count; i++)
                {
                    imeis[i] = HttpUtility.UrlDecode(imeis[i]);
                }
                //lấy trạng thái & thông tin imei
                model.result = _service.GetStateOfImeis(imeis);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/GetImeiState", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy thông tin về tình trạng của imei và vật tư
        /// update 2024-07-29: Nếu có param ma_kho => lấy trạng thái tồn kho theo mã kho chỉ định
        /// </summary>
        /// <param name="imeis">danh sách imei cần truy vấn</param>
        /// <returns></returns>
        [HttpPost("get_state_and_item")]
        #region GetImeiStateAndItem
        public IActionResult GetImeiStateAndItem([FromBody] List<string> imeis, [FromQuery] string? ma_kho)
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
                if (!_service.IsSQLInjectionValid(imeis.ToArray()) || !_service.IsSQLInjectionValid(ma_kho))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                imeis.ForEach(x => x = HttpUtility.UrlDecode(x));

                //2024-06-15: tạm bỏ qua đoạn code dưới để cho phép imei có chứa ký tự đặc biệt
                /*
                for (int i = 0; i < imeis.Count; i++)
                {
                    imeis[i] = HttpUtility.UrlDecode(imeis[i]);
                }
                */

                //lấy trạng thái & thông tin imei
                if(string.IsNullOrEmpty(ma_kho))
                {
                    model.result = _service.GetStateAndItemOfImeis(imeis);
                }
                else
                {
                    model.result = _service.GetStateItemOfImeisInStock(imeis, ma_kho); ;
                }

                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/GetImeiStateAndItem", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy danh sách imei có tồn kho theo mã vật tư tại cửa hàng (mã cửa hàng hiện thời lấy từ thông tin đăng nhập)
        /// </summary>
        /// <param name="ma_ct">Mã chứng từ</param>
        /// <param name="ma_vt">Mã vật tư cần lấy danh sách imei</param>
        /// <returns></returns>
        [HttpGet("getbyitem")]
        #region GetImeiState
        public IActionResult GetByItem(string ma_ct, string? ma_vt, string? ten_vt, string? ma_imei, string? ma_kho, int page_index = 1, int page_size = 0)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = new { }
                };
                Service _service = new Service();

                //check injection
                if (!_service.IsSQLInjectionValid(ma_ct) || !_service.IsSQLInjectionValid(ma_vt) || !_service.IsSQLInjectionValid(ten_vt) || !_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_kho))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy trạng thái & thông tin imei
                EntityCollection<Dictionary<string, object>> imeis = _service.GetImeisInStoreByItem(Startup.Shop, ma_ct, string.IsNullOrEmpty(ma_vt) ? "" : ma_vt, string.IsNullOrEmpty(ten_vt) ? "" : ten_vt, string.IsNullOrEmpty(ma_imei) ? "" : ma_imei, string.IsNullOrEmpty(ma_kho) ? "" : ma_kho, page_index, page_size);
                if (imeis != null && imeis.Items != null)
                {
                    model.success = true;
                    model.result = imeis;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/GetByItem", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lọc tìm danh sách imei theo key (cho phép tìm gần đúng)
        /// </summary>
        /// <param name="ma_vt">Mã vật tư cần lấy danh sách imei</param>
        /// <returns></returns>
        [HttpGet("filter")]
        #region FilterByKey
        public IActionResult FilterByKey(string key)
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
                if (!_service.IsSQLInjectionValid(key))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy trạng thái & thông tin imei
                model.result = _service.FindImeisByKey(Startup.Shop, key);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/FilterByKey", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Cập nhật trạng thái đặt hàng cho danh sách imei
        /// </summary>
        /// <returns></returns>
        [HttpPost("upsaleorder")]
        #region UpdateOrder
        public IActionResult UpdateSaleOrder([FromBody] ImeiStateInput input)
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
                if (!_service.IsSQLInjectionValid(input.imeis.ToArray()))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                for (int i = 0; i < input.imeis.Count; i++)
                {
                    input.imeis[i] = HttpUtility.UrlDecode(input.imeis[i]);
                }
                //lấy trạng thái & thông tin imei
                model.result = _service.UpdateImeiOrderState(Startup.Shop, input.imeis, input.state, input.ma_ct, input.nxt);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/UpdateSaleOrder", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpGet("soldinfo/")]
        #region GetImeiSoldInfo
        public IActionResult GetImeiSoldInfo(string ma_imei, string ma_cuahang, string? ma_ct = "", decimal? rate = null, decimal? tien_giam = 0, string? loai_tra_lai = "", bool? tra_lai_cod = false)
        {
            try
            {
                Service _service = new Service(_configuration);

                //check injection
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang) || !_service.IsSQLInjectionValid(loai_tra_lai))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                CommonObjectModel model = _service.GetImeiSoldInfo(ma_imei, ma_cuahang, ma_ct ?? "", rate ?? -1, tien_giam ?? 0, loai_tra_lai ?? "", tra_lai_cod ?? false);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/UpdateSaleOrder", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpGet("warranty-out-info/")]
        #region GetImeiSoldInfo
        public IActionResult GetWarrantyOutInfo(string ma_imei, string ma_cuahang)
        {
            try
            {
                Service _service = new Service(_configuration);

                //check injection
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                CommonObjectModel model = _service.GetImeiWarrantyOutInfo(ma_imei, ma_cuahang);

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/warranty-out-info", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion


        [HttpGet("change-gift-promotions")]
        #region ChangePromotionsForImei
        public IActionResult ChangePromotionsForImei(string ma_imei, string ma_ck, int rec)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };

                Service _service = new Service(_configuration);

                //check injection
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_ck))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                List<GiftItem> result = _service.GetAllGiftPromotionsForImei(ma_imei, ma_ck, rec);
                if(result != null && result.Count > 0)
                {
                    model.success = true;
                    model.result = result;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/ChangePromotionsForImei", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

    }
}
