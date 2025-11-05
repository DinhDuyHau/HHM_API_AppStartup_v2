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
using Genbyte.Base.Security;
using System.Diagnostics;

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
        public IActionResult GetImeiInStore(string ma_imei, string ma_cuahang, string ma_ct, string? ma_kh, DateTime? ngay_ct = null)
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
                DataSet ds = _service.GetImeiInStore(ma_imei, ma_cuahang, ma_ct, ma_kh, ngay_ct);
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
                            model.success = model.success && imei_state.ban_lk_yn;

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
        [HttpPost("state_and_item")]
        #region GetImeiStateAndItem
        public IActionResult ImeiStateAndItem([FromBody] List<string> imeis, [FromQuery] string? ma_kho)
        {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();

                //Kiểm tra Imei có thuộc kho hàng
                var kq = _service.CheckStock(imeis, ma_kho);
                var itemsWithFalseExists = kq.Where(e => !e.exists_yn).ToList();
                if (itemsWithFalseExists.Count != 0)
                {
                    model.result = itemsWithFalseExists;
                    model.message = "in_stock_yn_no";
                    return Ok(model);
                }

            return GetImeiStateAndItem(imeis, ma_kho);



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

        /**
         * call method GetSingleImeiStateAndItem thay đổi từ GET sang POST
         * param imeis sử dụng List vì nếu để string khi gửi request từ client sẽ bị lỗi 415
         */
        [HttpPost("single_imei_state")]
        #region SingleImeiStateAndItemWithPost
        public IActionResult SingleImeiStateAndItemWithPost([FromBody] List<string> imeis, [FromQuery] string? ma_kho)
        {
            //lấy ra imei đầu tiên trong danh sách để truy vấn thông tin
            //(chỉ xử lý truy vấn cho 1 imei nhưng do request param bắt buộc phải để dạng List)
            string ma_imei = imeis.FirstOrDefault() ?? "";

            return GetSingleImeiStateAndItem(ma_imei, ma_kho);
        }
        #endregion

        [HttpGet("get_single_imei_state")]
        #region GetSingleImeiStateAndItem
        public IActionResult GetSingleImeiStateAndItem([FromQuery] string imei, [FromQuery] string? ma_kho)
        {
            try
            {
                char seperator = '\x00ff';  //char(255)
                List<string> imeis = new List<string>();
                imeis.Add(imei);

                /*
                 * ĐOẠN DƯỚI COPY CODE TỪ METHOD GetImeiStateAndItem SỬA THÊM param seperator
                 */
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
                if (string.IsNullOrEmpty(ma_kho))
                {
                    model.result = _service.GetStateAndItemOfImeis(imeis, seperator);
                }
                else
                {
                    model.result = _service.GetStateItemOfImeisInStock(imeis, ma_kho, seperator); ;
                }

                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/GetSingleImeiStateAndItem", ex);
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

        [HttpGet("soldinfo_change_item/")]
        #region GetImeiSoldInfo
        public IActionResult GetImeiSoldInfoChGetImeiSoldInfoChangeItemange(string ma_imei, string ma_cuahang, string? ma_ct = "", decimal? rate = null, decimal? tien_giam = 0, string? loai_tra_lai = "", bool? tra_lai_cod = false)
        {
            try
            {
                Service _service = new Service(_configuration);

                //check injection
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang) || !_service.IsSQLInjectionValid(loai_tra_lai))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                CommonObjectModel model = _service.GetImeiSoldInfoChangeItem(ma_imei, ma_cuahang, ma_ct ?? "", rate ?? -1, tien_giam ?? 0, loai_tra_lai ?? "", tra_lai_cod ?? false);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/GetImeiSoldInfoChangeItem", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpGet("soldinfo_return/")]
        #region GetImeiSoldInfoReturn
        public IActionResult GetImeiSoldInfoReturn(string ma_imei, string ma_cuahang, string? ma_ct = "", decimal? rate = null, decimal? tien_giam = 0, string? loai_tra_lai = "", bool? tra_lai_cod = false, bool? tra_lai_freedelivery = false, bool? dieu_chinh_gia = false)
        {
            try
            {
                Service _service = new Service(_configuration);

                //check injection
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang) || !_service.IsSQLInjectionValid(loai_tra_lai))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                CommonObjectModel model = _service.GetImeiSoldInfoReturn(ma_imei, ma_cuahang, ma_ct ?? "", rate ?? -1, tien_giam ?? 0, loai_tra_lai ?? "", tra_lai_cod ?? false, tra_lai_freedelivery ?? false, dieu_chinh_gia ?? false);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/GetImeiSoldInfoReturn", ex);
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
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };

                Service _service = new Service(_configuration);

                //check injection
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                ma_imei = HttpUtility.UrlDecode(ma_imei);
                //lấy trạng thái & thông tin imei
                List<ImeiWarrantyOut> entities = _service.GetImeiWarrantyOutInfo(ma_imei, ma_cuahang);
                if (entities != null && entities.Count > 0)
                {
                    entities.ForEach(x =>
                    {
                        x.stt_rec_px = APIService.EncryptForWebApp(x.stt_rec_px, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                    });

                    model.success = true;
                    model.result = entities;
                }
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
        public IActionResult ChangePromotionsForImei(string ma_imei, string ma_ck, int rec, string ma_vt_tang)
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
                List<GiftItem> result = _service.GetAllGiftPromotionsForImei(ma_imei, ma_ck, rec, ma_vt_tang);
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

        [HttpGet("search-imei-warranty")]
        #region SearchImeiWarranty
        public IActionResult SearchImeiWarranty(string ma_imei, string ma_cuahang)
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
                if (!_service.IsSQLInjectionValid(ma_imei) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                ma_imei = HttpUtility.UrlDecode(ma_imei);

                //tìm kiếm imei bảo hành
                List<ImeiWarrantyOut> entities = _service.SearchImeiWarranty(ma_imei, ma_cuahang);
                if (entities != null && entities.Count > 0)
                {
                    entities.ForEach(x =>
                    {
                        x.stt_rec_px = APIService.EncryptForWebApp(x.stt_rec_px, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                    });

                    model.success = true;
                    model.result = entities;
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- ImeiController/search-imei-warranty", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Tìm danh sách imei theo ma_imei hoặc cửa hàng nếu truyền vào (cho phép tìm gần đúng)
        /// </summary>
        /// <param name="ma_imei">Mã imei cần tìm</param>
        /// <param name="ma_cuahang">Mã cửa hàng cần tìm cùng imei</param>
        /// <returns></returns>
        [HttpGet("find_by_prefix")]
        #region FindByPrefix
        public IActionResult FindByPrefix(string ma_imei, string? ma_cuahang = null, bool isCheckInventory = true, int page_index = 1, int page_size = 0)
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
                if (!_service.IsSQLInjectionValid(ma_imei))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                if (!_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //thông tin imei: kho, vật tư
                model.result = _service.FindImeiByPrefix(ma_imei, ma_cuahang, isCheckInventory, page_index, page_size);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/FindByPrefix", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy thông tin ck 09
        /// </summary>
        /// <param name="ma_kh"></param>
        /// <param name="ma_hang"></param>
        /// <param name="ngay_ct"></param>
        /// <param name="ma_imei"></param>
        /// <param name="ma_vt"></param>
        /// <returns></returns>
        [HttpGet("discount_rank_customer")]
        #region GetDiscountRankCustomer
        public IActionResult GetDiscountRankCustomer(string ma_kh, string ma_hang, DateTime ngay_ct, string ma_imei, string ma_vt, string ma_ct = "")
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
                if (!_service.IsSQLInjectionValid(ma_hang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                if (!_service.IsSQLInjectionValid(ma_imei))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });
                if (!_service.IsSQLInjectionValid(ma_vt))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                model.result = _service.GetDiscountRankCustomer(ma_kh, ma_hang, ngay_ct, ma_imei, ma_vt, ma_ct);
                if (model.result != null)
                    model.success = true;

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ImeiController/GetDiscountRankCustomer", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
