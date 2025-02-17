using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Genbyte.Component.Voucher;
using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Voucher.SVTran.Models;
using Genbyte.Component.Voucher.Model;
using Genbyte.Base.Security;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Voucher.SVTran
{
    public class Service : ISVTranService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "BHA";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "c581$000000";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "m581$";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "i581$";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "d581$";
        private const string _DETAIL_PARA = "d581";

        // bảng lưu dữ liệu chi tiết của dịch vụ
        public string ServicesTable { get; } = "d581dv$";
        private const string _SERVICES_PARA = "d581dv";
        // bảng lưu dữ liệu chi tiết của chiết khấu
        public string DiscountTable { get; } = "d581ck$";
        private const string _DISCOUNT_PARA = "d581ck";
        // bảng lưu dữ liệu chi tiết của thanh toán
        public string PaidTable { get; } = "d581tt$";
        private const string _PAID_PARA = "d581tt";
        // bảng lưu dữ liệu chi tiết của bảo hành
        public string WarrantyTable { get; } = "d581bh$";
        private const string _WARRANTY_PARA = "d581bh";
        public string DetailKMTable { get; } = "r590_km$";

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "ma_kh,ma_dvcs,ma_cuahang,ma_ca;#10$,#20$,#30$, #40$; , , , :ma_kho,ma_vt,ma_imei;#10$,#20$,#30$;d581,d581,d581";

        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '000000', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(ma_ca) as ma_ca,ngay_ct,rtrim(so_ct) as so_ct,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang, rtrim(Dien_giai) as Dien_giai,t_tien_nt, t_tien_nt2,t_ck_nt,t_thue_nt,t_tt_nt,rtrim(ma_nt) as ma_nt,rtrim(ma_ct) as ma_ct,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(a.ma_ca) as ma_ca,c.ten_ca, rtrim(a.ma_cuahang) as ma_cuahang, ngay_ct,rtrim(so_ct) as so_ct,rtrim(a.ma_kh) as ma_kh,b.ten_kh,rtrim(a.dien_giai) as dien_giai,t_tien_nt,t_tien_nt2,t_ck_nt,t_thue_nt,t_tt_nt,rtrim(ma_nt) as ma_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.name,y.comment,z.comment2,'''' as Hash', 'a left join vdmkh_acc b on a.ma_kh = b.ma_kh left join dmca c on a.ma_ca = c.ma_ca left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "'''";

        /// <summary>
        /// Khai báo các hành động của user tác động đến service hiện tại: addnew, edit, read, delete
        /// </summary>
        public UserAction Action { get; set; }

        /// <summary>
        /// Khai báo quyền truy cập cho các xử lý CRUD
        /// </summary>
        public AccessRight VoucherRight { get; set; }
        private readonly IConfiguration _configuration;

        // Lấy danh sách imei xóa khỏi grid
        List<string> list_imei_delete = new List<string>();

        public Service(IConfiguration configuration)
        {
            VoucherRight = new AccessRight();
            VoucherRight.AllowRead = true;
            VoucherRight.AllowCreate = true;
            VoucherRight.AllowUpdate = true;
            VoucherRight.AllowDelete = true;
            this._configuration = configuration;
        }

        #region Inserting
        public CommonObjectModel Inserting(BaseModel data)
        {
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };

            /**
             * CONVERT DỮ LIỆU GỬI LÊN TỪ CLIENT SANG OBJECT
             */
            VoucherItem vc_item = Converter.BaseModelToEntity<VoucherItem>(data, this.Action);
            if (vc_item == null) return null;
            vc_item.ma_ct = this.VoucherCode;

            if (vc_item.ma_nt == "" || vc_item.ma_nt == null)
            {
                vc_item.ma_nt = "VND";
                vc_item.ty_gia = 1;
            }

            //Cập nhật ngày chứng từ là ngày hiện thời của Server
            vc_item.ngay_ct = DateTime.Today;
            vc_item.ngay_lct = DateTime.Today;


            //khai báo list chi tiết hàng hóa và list chi tiết dịch vụ (for check)
            List<SVDetail>? list_hang_hoa = null;
            List<SVServiceModel>? list_dich_vu = null;
            List<SVWarrantyModel>? list_goi_cuoc = null;

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: SVDetail
            int index_value = 1;
            if (data.details == null)
            {
                return null;
            }
            foreach (var item in data.details)
            {
                if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
                {
                    DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                    VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                    if (item_model != null && item_detail != null)
                    {
                        switch (index_value)
                        {
                            case 1:
                                List<SVDetail>? detail_list = JsonSerializer.Deserialize<List<SVDetail>>((JsonElement)item_model.Data);
                                if (detail_list != null && detail_list.Count > 0)
                                {
                                    // check hàng khuyến mại
                                    CheckKhuyenMai(detail_list);

                                    // check tổng tiền hàng

                                    //cập nhật ngày chứng từ
                                    detail_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(detail_list);
                                }
                                item_detail.Detail_Type = typeof(SVDetail).Name;
                                list_hang_hoa = detail_list;
                                break;
                            case 2:
                                List<SVServiceModel>? services_list = JsonSerializer.Deserialize<List<SVServiceModel>>((JsonElement)item_model.Data);
                                if (services_list != null && services_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    services_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(services_list);
                                }
                                item_detail.Detail_Type = typeof(SVServiceModel).Name;
                                list_dich_vu = services_list;
                                break;
                            case 3:
                                List<SVDiscountModel>? discount_list = JsonSerializer.Deserialize<List<SVDiscountModel>>((JsonElement)item_model.Data);
                                if (discount_list != null && discount_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    discount_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(discount_list);
                                }
                                item_detail.Detail_Type = typeof(SVDiscountModel).Name;
                                break;
                            case 4:
                                List<SVPaidModel>? paid_list = JsonSerializer.Deserialize<List<SVPaidModel>>((JsonElement)item_model.Data);
                                if (paid_list != null && paid_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    paid_list.ForEach(x => {
                                        x.ngay_ct = vc_item.ngay_ct;
                                        x.stt_rec_pt = APIService.DecryptForWebApp(x.stt_rec_pt, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                                    });

                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(paid_list);
                                }
                                item_detail.Detail_Type = typeof(SVPaidModel).Name;
                                break;
                            case 5:
                                List<SVWarrantyModel>? warranty_list = JsonSerializer.Deserialize<List<SVWarrantyModel>>((JsonElement)item_model.Data);
                                if (warranty_list != null && warranty_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    warranty_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(warranty_list);
                                }
                                item_detail.Detail_Type = typeof(SVWarrantyModel).Name;
                                list_goi_cuoc = warranty_list;
                                break;
                            default:
                                break;
                        }

                    }
                }
                index_value++;
            }

            // check tiền hợp lệ hay ko, phải khớp với nhau ko được lệch
            if (validMoneyMerchandise(list_hang_hoa, list_dich_vu, list_goi_cuoc, vc_item))
            {
                result_model.success = false;
                result_model.message = "invalid_money_merchandise";
                return result_model;
            }

            // check tổng tiền thanh toán với tiền còn nợ và tổng tiền đã thanh toán
            if (validTotalPayment(vc_item))
            {
                result_model.success = false;
                result_model.message = "invalid_total_payment";
                return result_model;
            }

            //check các trường số lượng, giá, tiền trong grid hàng hóa và dịch vụ
            bool check_quantity_amount = true;
            if (list_hang_hoa != null && list_hang_hoa.Count > 0)
                foreach(SVDetail? item in list_hang_hoa)
                {
                    if (item.so_luong < 0 || item.gia2 < 0 || item.gia_ck < 0 || item.tien2 < 0 || item.thue < 0 || item.tt < 0)
                        check_quantity_amount = false;
                    break;
                }
            if (list_dich_vu != null && list_dich_vu.Count > 0)
                foreach(SVServiceModel? service_item in list_dich_vu)
                {
                    if (service_item.so_luong < 0 || service_item.gia2 < 0 || service_item.gia_ck < 0 || service_item.tien2 < 0 || service_item.thue < 0 || service_item.tt < 0)
                        check_quantity_amount = false;
                    break;

                }
            if(!check_quantity_amount)
            {
                result_model.success = false;
                result_model.message = "voucher_quantity_amount_is_negative_number";
                return result_model;
            }


            //Check tồn  trạng thái chứng từ thuộc danh sách trạng thái được phép thêm
            string sql = @"DECLARE @check TABLE (
	            is_success BIT,
	            message VARCHAR(100)
            )
            DECLARE @status_older CHAR(1)
            INSERT INTO @check (is_success, message) VALUES (1, '')

            IF NOT EXISTS(SELECT 1 FROM dmttct WHERE ma_ct = @vc_code AND status = @vc_status) BEGIN
                UPDATE @check SET is_success = 0, message = 'status_not_exists'
	            SELECT * FROM @check
	            RETURN
            END

            IF NOT EXISTS(SELECT 1 FROM dmttct WHERE (xdefault = 1 OR right_yn = 1) AND ma_ct = @vc_code AND status = @vc_status) BEGIN
	            UPDATE @check SET is_success = 0, message = 'status_cannot_create'
	            SELECT * FROM @check
	            RETURN
            END

            SELECT is_success, message FROM @check";
            CoreService service = new CoreService();
            List<SqlParameter> paras = new List<SqlParameter>();
            #region add parameters
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.stt_rec.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_code",
                SqlDbType = SqlDbType.Char,
                Value = this.VoucherCode
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_status",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.status.Replace("'", "''")
            });
            #endregion
            CheckResult check_result = service.ExecSql2List<CheckResult>(sql, paras).FirstOrDefault()!;
            if (!check_result.is_success)
            {
                result_model.success = false;
                result_model.message = check_result.message;
                return result_model;
            }

            result_model = CommonService.checkPaid(vc_item, _PAID_PARA);
            if (!result_model.success)
            {
                return result_model;
            }
            result_model = CommonService.checkImeiInsert(vc_item, _DETAIL_PARA);
            if (!result_model.success)
            {
                return result_model;
            }
            result_model = CommonService.checkDiscount(vc_item, _DISCOUNT_PARA);
            if (!result_model.success)
            {
                return result_model;
            }
            result_model.result = vc_item;
            return result_model;
        }
        #endregion

        #region Inserted
        //Thực hiện công việc insert tiếp theo sau khi đã render ra chuỗi truy vấn (đối số query)
        public CommonObjectModel Inserted(object voucherItem, Query voucherQuery, int user_id)
        {
            if (voucherItem == null || voucherQuery == null)
                throw new Exception(ApiReponseMessage.Error_notExist);

            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };

            /**
             * CHECK DỮ LIỆU HỢP LỆ
             */
            if (voucherItem == null || voucherQuery == null || string.IsNullOrEmpty(voucherQuery.Prime))
            {
                model.success = false;
                model.message = "err_cast_input_to_object";
                return model;
            }
            VoucherItem vc_item = (VoucherItem)voucherItem;

            //check voucher date
            if (vc_item.ngay_ct == null)
            {
                model.success = false;
                model.message = "voucher_date_null";
            }

            //CheckLockedDate
            //CheckVoucherNumber

            //create query
            string query = voucherQuery.Prime;

            //Tạo stt_rec (PK)
            query += "\n\n";
            query += VoucherUtils.GetQueryCreateIdentityNumber(this.VoucherCode, this.MasterTable);

            //insert prime
            string expression = vc_item.ngay_ct?.ToString("yyyyMM");
            string prime_table = this.PrimeTable.Trim() + expression;
            string insert_prime_table_query = VoucherUtils.getMaterQuery(new VoucherItem(), prime_table, user_id, 1);
            query += "\n\n";
            //query += $"insert into {prime_table} (stt_rec, ma_ct, so_ct, ngay_ct, ngay_lct, ma_gd, loai_ct, ma_kh, ma_nt, ty_gia, t_so_luong, t_tien, t_tien_nt, t_tien2, t_tien_nt2, t_tt, t_tt_nt, ma_thue, t_thue_nt, t_thue, status, ma_dvcs, ma_cuahang, ma_ca, dien_giai, user_id0, user_id2, datetime0, datetime2, t_ck, t_da_tra, t_con_no, tien_dat_coc, ma_nvvc, ma_nvbh) ";
            //query += $" select @stt_rec, @ma_ct, @so_ct, @ngay_ct, @ngay_ct, @ma_gd, @loai_ct, @ma_kh, @ma_nt, @ty_gia, @t_so_luong, @t_tien, @t_tien_nt, @t_tien2, @t_tien_nt2, @t_tt, @t_tt_nt, @ma_thue, @t_thue_nt, @t_thue_nt, @status, @ma_dvcs, @ma_cuahang, @ma_ca, @dien_giai, {user_id}, {user_id}, getdate(), getdate(), @t_ck, @t_da_tra, @t_con_no, @tien_dat_coc, @ma_nvvc, @ma_nvbh";
            query += $"{insert_prime_table_query}";

            //insert các bảng chi tiết
            DetailQuery? detail_query = null;
            string detail_table = "", services_table = "", discount_table = "", paid_table = "", warranty_table = "";
            if (voucherQuery.Details.Any(x => x.ParaName == _DETAIL_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DETAIL_PARA);
                detail_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_DETAIL_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";
                query += $"insert into {detail_table} (stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,km_yn,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv,tk_vt,so_luong,gia_nt,gia,gia_nt2,gia2,tien_nt,tien,thue,thue_nt,tt,tt_nt,xstatus,xaction,tien2,tien_nt2,cp_bh,cp_bh_nt,cp_vc,cp_vc_nt,cp_khac,cp_khac_nt,cp,cp_nt,stt_rec_ct,stt_rec0ct,gia_ban0,gia_ban_nt0,gia_ban,gia_ban_nt,tl_ck,gia_ck,gia_ck_nt,ck,ck_nt,sl_dh,sl_xuat,sl_giao,stt_rec_dh,stt_rec0dh,dh_so,dh_ln,stt_rec_px,stt_rec0px,px_so,px_ln,stt_rec_gh,stt_rec0gh,stt_rec_pn,stt_rec0pn,tk_gv,tk_dt,tk_ck,tk_cpbh,px_gia_dd,ma_nvbh_i,line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_thue,tk_thue_no,tk_thue_co,thue_suat,ma_imei,no_km_yn,ma_vt_kmtt,tien_kmqd,imei_mua, gia_vat,tl_ck09,tien_kb09,tien_max09,tien_ck09) select stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,km_yn,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv,tk_vt,so_luong,gia_nt,gia,gia_nt2,gia2,tien_nt,tien,thue,thue_nt,tt,tt_nt,xstatus,xaction,tien2,tien_nt2,cp_bh,cp_bh_nt,cp_vc,cp_vc_nt,cp_khac,cp_khac_nt,cp,cp_nt,stt_rec_ct,stt_rec0ct,gia_ban0,gia_ban_nt0,gia_ban,gia_ban_nt,tl_ck,gia_ck,gia_ck_nt,ck,ck_nt,sl_dh,sl_xuat,sl_giao,stt_rec_dh,stt_rec0dh,dh_so,dh_ln,stt_rec_px,stt_rec0px,px_so,px_ln,stt_rec_gh,stt_rec0gh,stt_rec_pn,stt_rec0pn,tk_gv,tk_dt,tk_ck,tk_cpbh,px_gia_dd,ma_nvbh_i,line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_thue,tk_thue_no,tk_thue_co,thue_suat,ma_imei,no_km_yn,ma_vt_kmtt,tien_kmqd,imei_mua, gia_vat, tl_ck09,tien_kb09,tien_max09,tien_ck09 from @{_DETAIL_PARA} \r\n";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _SERVICES_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _SERVICES_PARA);
                services_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string insert_service_query = VoucherUtils.getServiceQuery(new SVServiceModel(), services_table, _SERVICES_PARA, 1);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_SERVICES_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";
                //query += $"insert into {services_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_dv, km_yn, ma_sp, ma_bp, so_lsx, dvt, he_so, ma_kho, ma_vi_tri, ma_lo, ma_vv, tk_vt, so_luong, gia_nt, gia, gia_nt2, gia2, tien_nt, tien, thue, thue_nt, tt, tt_nt, xstatus, xaction, tien2, tien_nt2, cp_bh, cp_bh_nt, cp_vc, cp_vc_nt, cp_khac, cp_khac_nt, cp, cp_nt, stt_rec_ct, stt_rec0ct, gia_ban0, gia_ban_nt0, gia_ban, gia_ban_nt, tl_ck, gia_ck, gia_ck_nt, ck, ck_nt, sl_dh, sl_xuat, sl_giao, stt_rec_dh, stt_rec0dh, dh_so, dh_ln, stt_rec_px, stt_rec0px, px_so, px_ln, stt_rec_gh, stt_rec0gh, stt_rec_pn, stt_rec0pn, tk_gv, tk_dt, tk_ck, tk_cpbh, px_gia_dd, ma_nvbh_i, ma_hd, ma_ku, ma_phi, so_dh_i, ma_td1, ma_td2, ma_td3, ma_thue, tk_thue_no, tk_thue_co, thue_suat, ma_imei) select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_dv, km_yn, ma_sp, ma_bp, so_lsx, dvt, he_so, ma_kho, ma_vi_tri, ma_lo, ma_vv, tk_vt, so_luong, gia_nt, gia, gia_nt2, gia2, tien_nt, tien, thue, thue_nt, tt, tt_nt, xstatus, xaction, tien2, tien_nt2, cp_bh, cp_bh_nt, cp_vc, cp_vc_nt, cp_khac, cp_khac_nt, cp, cp_nt, stt_rec_ct, stt_rec0ct, gia_ban0, gia_ban_nt0, gia_ban, gia_ban_nt, tl_ck, gia_ck, gia_ck_nt, ck, ck_nt, sl_dh, sl_xuat, sl_giao, stt_rec_dh, stt_rec0dh, dh_so, dh_ln, stt_rec_px, stt_rec0px, px_so, px_ln, stt_rec_gh, stt_rec0gh, stt_rec_pn, stt_rec0pn, tk_gv, tk_dt, tk_ck, tk_cpbh, px_gia_dd, ma_nvbh_i, ma_hd, ma_ku, ma_phi, so_dh_i, ma_td1, ma_td2, ma_td3, ma_thue, tk_thue_no, tk_thue_co, thue_suat, ma_imei from @{_SERVICES_PARA}";
                query += $"{insert_service_query}";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _DISCOUNT_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DISCOUNT_PARA);
                discount_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string insert_discount_query = VoucherUtils.getDiscountQuery(new SVDiscountModel(), discount_table, _DISCOUNT_PARA, 1);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_DISCOUNT_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";
                //query += $"insert into {discount_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_ck, ngay_bd, ngay_kt, tl_ck, tien_ck, ma_nvbh_i, ma_vt, ma_vt_tang, sl_tang, sl_vt_tt, ma_hang_tt, tien_qd, imei_hang_mua, ma_imei) select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_ck, ngay_bd, ngay_kt, tl_ck, tien_ck, ma_nvbh_i, ma_vt, ma_vt_tang, sl_tang, sl_vt_tt, ma_hang_tt, tien_qd, imei_hang_mua, ma_imei from @{_DISCOUNT_PARA}";
                query += $"{insert_discount_query}";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _PAID_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _PAID_PARA);
                paid_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string insert_paid_query = VoucherUtils.getPaidQuery(new SVPaidModel(), paid_table, _PAID_PARA, 1);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_PAID_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";
                //query += $"insert into {paid_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_thanhtoan, ma_nvbh_i, tien, tien_nt, so_the_nh, ma_chuan_chi, ma_may_pos, tk_nh_nhan, so_hd_vnpay, vi_dien_tu, so_hd_tragop, tien_phi_bh, ma_dv_tragop) select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_thanhtoan, ma_nvbh_i, tien, tien_nt, so_the_nh, ma_chuan_chi, ma_may_pos, tk_nh_nhan, so_hd_vnpay, vi_dien_tu, so_hd_tragop, tien_phi_bh, ma_dv_tragop from @{_PAID_PARA}";
                query += $"{insert_paid_query}";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _WARRANTY_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _WARRANTY_PARA);
                warranty_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string insert_warranty_query = VoucherUtils.getWarrantyQuery(new SVWarrantyModel(), warranty_table, _WARRANTY_PARA, 1);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_WARRANTY_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";
                //query += $"insert into {warranty_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, ma_ttbh, ma_imei, hang_sx, ma_vt) select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, ma_ttbh, ma_imei, hang_sx, ma_vt from @{_WARRANTY_PARA}";
                query += $"{insert_warranty_query}";
            }

            //insert km
            var sale_table = "r590_km$" + expression;
            string insert_promotion_query = VoucherUtils.getPromotionQuery(sale_table, _DETAIL_PARA, user_id, 1);
            query += "\n";
            //query += $"insert into {sale_table} (stt_rec,stt_rec0,ma_ct,ma_dvcs,ma_kh,sl_ban,no_qua_yn,ngay_ct,so_ct,ma_kho,ma_vt,dvt,line_nbr,ma_imei,datetime0,datetime2,user_id0,user_id2) ";
            //query += $" select stt_rec,stt_rec0,ma_ct,@ma_dvcs,@ma_kh,so_luong,no_km_yn,ngay_ct,so_ct,ma_kho,ma_vt,dvt,line_nbr,ma_imei, getdate(), getdate(),0, 0 from @{_DETAIL_PARA} where km_yn = 1";
            query += $"{insert_promotion_query}";

            query += "\n\n";
            query += "select @stt_rec as stt_rec, @ma_ct as ma_ct";

            //thực thi query insert vào bảng prime và detail có sử dụng transaction
            CoreService service = new CoreService();
            DataSet ds = service.ExecTransactionSql2DataSet(query);
            string stt_rec = ds.Tables[0].Rows[0]["stt_rec"].ToString();
            string ma_ct = ds.Tables[0].Rows[0]["ma_ct"].ToString();

            //update stt_rec cho đối tượng đang thực hiện thêm mới
            vc_item.stt_rec = stt_rec;
            //up stt_rec --> chi tiết của đối tượng
            foreach (VoucherDetail vc_detail in vc_item.details)
            {
                if (vc_detail == null || vc_detail.Data == null || vc_detail.Data.Count <= 0)
                    continue;
                vc_detail.Data.ForEach(x => x.stt_rec = stt_rec);
            }

            //update các trường null
            query = $"exec fs_UpdateNullToTable '{prime_table}', '{prime_table}', 'stt_rec = ''{stt_rec}''' \n";
            query += $"exec fs_UpdateNullToTable '{detail_table}', '{detail_table}', 'stt_rec = ''{stt_rec}''' \n";
            if (services_table != "")
                query += $"exec fs_UpdateNullToTable '{services_table}', '{services_table}', 'stt_rec = ''{stt_rec}''' \n";
            if (discount_table != "")
                query += $"exec fs_UpdateNullToTable '{discount_table}', '{discount_table}', 'stt_rec = ''{stt_rec}''' \n";
            if (paid_table != "")
                query += $"exec fs_UpdateNullToTable '{paid_table}', '{paid_table}', 'stt_rec = ''{stt_rec}''' \n";
            if (warranty_table != "")
                query += $"exec fs_UpdateNullToTable '{warranty_table}', '{warranty_table}', 'stt_rec = ''{stt_rec}''' \n";
            if (!string.IsNullOrEmpty(sale_table))
            {
                query += $"exec fs_UpdateNullToTable '{sale_table}', '{sale_table}', 'stt_rec = ''{stt_rec}''' \n";
            }
            service.ExecuteNonQuery(query);

            //Update dữ liệu thanh toán và key đối với dịch vụ
            CommonService.updatePaid(vc_item, _PAID_PARA);
            CommonService.updateService(vc_item, _SERVICES_PARA, ServicesTable, vc_item.email_nhan_key);

            //insert bảng master (c) & inquiry (i)
            string inquiry_table = this.InquiryTable.Trim() + expression;
            query = $"exec MokaOnline$App$Voucher$UpdateInquiryTable '{this.VoucherCode}', '{inquiry_table}', '{prime_table}', '{detail_table}', 'stt_rec', '{stt_rec}', '{this.Operation}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateGrandTable '{this.VoucherCode}', '{this.MasterTable}', '{prime_table}', 'stt_rec', '{stt_rec}'";
            service.ExecuteNonQuery(query);

            model.success = true;
            model.message = "";
            model.result = vc_item;
            return model;
        }
        #endregion

        #region Updating

        public CommonObjectModel Updating(BaseModel data)
        {
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };

            /**
             * CONVERT DỮ LIỆU GỬI LÊN TỪ CLIENT SANG OBJECT
             */
            VoucherItem vc_item = Converter.BaseModelToEntity<VoucherItem>(data, this.Action);

            //tạm cho phép cập nhật ngày quá khứ
            /*
            if (vc_item.ngay_ct.Value.Date != DateTime.Today)
            {
                result_model.success = false;
                result_model.message = "voucher_cannot_edit";
                return result_model;
            }
            */

            if (vc_item == null) return null;
            vc_item.ma_ct = this.VoucherCode;

            if (vc_item.ma_nt == "" || vc_item.ma_nt == null)
            {
                vc_item.ma_nt = "VND";
                vc_item.ty_gia = 1;
            }

            List<SVDetail>? list_hang_hoa = null;
            List<SVServiceModel>? list_dich_vu = null;
            List<SVWarrantyModel>? list_goi_cuoc = null;

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: SVDetail
            int index_value = 1;
            // Lấy danh sách tất cả các imei
            List<string> imeis = new List<string>();
            foreach (var item in data.details)
            {
                if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
                {
                    DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                    VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);
                    if (item_model != null && item_detail != null)
                    {
                        switch (index_value)
                        {
                            case 1:
                                List<SVDetail>? detail_list = JsonSerializer.Deserialize<List<SVDetail>>((JsonElement)item_model.Data);
                                if (detail_list != null && detail_list.Count > 0)
                                {
                                    // check hàng khuyến mại
                                    CheckKhuyenMai(detail_list);

                                    detail_list.ForEach((item) =>
                                    {
                                        if (item.ma_imei != null && item.ma_imei != "")
                                        {
                                            imeis.AddRange(item.ma_imei.Split(",").ToList().Select(x => x.Trim()));
                                        }
                                    });
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(detail_list);
                                }
                                item_detail.Detail_Type = typeof(SVDetail).Name;
                                list_hang_hoa = detail_list;
                                break;
                            case 2:
                                List<SVServiceModel>? service_list = JsonSerializer.Deserialize<List<SVServiceModel>>((JsonElement)item_model.Data);
                                if (service_list != null && service_list.Count > 0)
                                {
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(service_list);
                                }
                                item_detail.Detail_Type = typeof(SVServiceModel).Name;
                                list_dich_vu = service_list;
                                break;
                            case 3:
                                List<SVDiscountModel>? discount_list = JsonSerializer.Deserialize<List<SVDiscountModel>>((JsonElement)item_model.Data);
                                if (discount_list != null && discount_list.Count > 0)
                                {
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(discount_list);
                                }
                                item_detail.Detail_Type = typeof(SVDiscountModel).Name;
                                break;
                            case 4:
                                List<SVPaidModel>? paid_list = JsonSerializer.Deserialize<List<SVPaidModel>>((JsonElement)item_model.Data);
                                if (paid_list != null && paid_list.Count > 0)
                                {
                                    paid_list.ForEach(x => {
                                        x.stt_rec_pt = APIService.DecryptForWebApp(x.stt_rec_pt,  _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                                    });
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(paid_list);
                                }
                                item_detail.Detail_Type = typeof(SVPaidModel).Name;
                                break;
                            case 5:
                                List<SVWarrantyModel>? warranty_list = JsonSerializer.Deserialize<List<SVWarrantyModel>>((JsonElement)item_model.Data);
                                if (warranty_list != null && warranty_list.Count > 0)
                                {
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(warranty_list);
                                }
                                item_detail.Detail_Type = typeof(SVWarrantyModel).Name;
                                list_goi_cuoc = warranty_list;
                                break;
                            default:
                                break;
                        }
                    }
                }
                index_value++;
            }

            // check tiền hợp lệ hay ko, phải khớp với nhau ko được lệch
            if (validMoneyMerchandise(list_hang_hoa, list_dich_vu, list_goi_cuoc, vc_item))
            {
                result_model.success = false;
                result_model.message = "invalid_money_merchandise";
                return result_model;
            }

            // check tổng tiền thanh toán với tiền còn nợ và tổng tiền đã thanh toán
            if (validTotalPayment(vc_item))
            {
                result_model.success = false;
                result_model.message = "invalid_total_payment";
                return result_model;
            }

            //2024-10-28: kiểm tra imei duplicate
            Dictionary<string, int> imei_group_count = imeis.Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x).Select(y => new
            {
                y.Key,
                Count = y.Count()
            }).OrderByDescending(x => x.Count).ToDictionary(z => z.Key, z => z.Count);
            if (imei_group_count.Any(x => x.Value > 1))
            {
                result_model.success = false;
                result_model.message = "err_imei_duplicate";
                return result_model;
            }

            //Check tồn tại chứng từ & trạng thái chứng từ thuộc danh sách trạng thái được phép sửa
            string sql = @"DECLARE @check TABLE (
	is_success BIT,
	message VARCHAR(100)
)
DECLARE @status_older CHAR(1)
INSERT INTO @check (is_success, message) VALUES (1, '')
SELECT @status_older = status FROM " + this.MasterTable + @" WHERE stt_rec = @vc_id
IF @status_older is NULL
BEGIN
	UPDATE @check SET is_success = 0, message = 'voucher_not_exists'
	SELECT * FROM @check
	RETURN
END

IF NOT EXISTS(SELECT 1 FROM dmttct WHERE ma_ct = @vc_code AND status = @vc_status) BEGIN
    UPDATE @check SET is_success = 0, message = 'status_change_not_exists'
	SELECT * FROM @check
	RETURN
END

IF @status_older <> '0' BEGIN
    UPDATE @check SET is_success = 0, message = 'status_changed_cannot_update'
	SELECT * FROM @check
	RETURN
END

IF NOT EXISTS(SELECT 1 FROM dmttct WHERE (xdefault = 1 OR xedit = 1) AND ma_ct = @vc_code AND status = @status_older) BEGIN
	UPDATE @check SET is_success = 0, message = 'status_changed_cannot_update'
	SELECT * FROM @check
	RETURN
END

SELECT is_success, message FROM @check";
            CoreService service = new CoreService();
            List<SqlParameter> paras = new List<SqlParameter>();
            #region add parameters
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.stt_rec.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_code",
                SqlDbType = SqlDbType.Char,
                Value = this.VoucherCode
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_status",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.status.Replace("'", "''")
            });
            #endregion
            CheckResult check_result = service.ExecSql2List<CheckResult>(sql, paras).FirstOrDefault()!;
            if (!check_result.is_success)
            {
                result_model.success = false;
                result_model.message = check_result.message;
                return result_model;
            }
            //if (vc_item.status == "2")
            //{
            //    var imeiService = new Imei.Service();
            //    List<Imei.ImeiState> state_imei = imeiService.GetStateOfImeis(imeis);
            //    Imei.ImeiState? exists = state_imei.FirstOrDefault(x => x.exists_yn == false);
            //    if (exists != null)
            //    {
            //        result_model.success = false;
            //        result_model.message = "imei_not_exists";
            //        return result_model;
            //    }
            //}

            /**
             * Lấy thông tin chứng từ cũ trước khi thực hiện update
             */
            //sql = "EXEC Genbyte$System$GetVoucherPrimeInfo @vc_id, @vc_code";
            //paras = new List<SqlParameter>();
            //#region add parameters
            //paras.Add(new SqlParameter()
            //{
            //    ParameterName = "@vc_id",
            //    SqlDbType = SqlDbType.Char,
            //    Value = vc_item.stt_rec.Replace("'", "''")
            //});
            //paras.Add(new SqlParameter()
            //{
            //    ParameterName = "@vc_code",
            //    SqlDbType = SqlDbType.Char,
            //    Value = this.VoucherCode
            //});
            //#endregion
            BaseModel res = (BaseModel) this.GetById(vc_item.stt_rec.Replace("'", "''")).result;
            VoucherItem old_voucher = (VoucherItem) res.masterInfo;
            //VoucherItem? old_voucher = service.ExecSql2List<VoucherItem>(sql, paras).FirstOrDefault();
            if (old_voucher != null)
            {
                //Gán mã ca theo thông tin đăng nhập
                vc_item.ma_ca = Startup.Shift;

                //Gán lại các thông tin từ chứng từ cũ trước khi sửa: ma_dvcs, ma_cuahang, ma_ca, ngay_ct
                //(không cho sửa các trường này)
                vc_item.ma_dvcs = old_voucher.ma_dvcs;
                vc_item.ma_cuahang = old_voucher.ma_cuahang;
                vc_item.ngay_ct = old_voucher.ngay_ct;
                vc_item.ngay_lct = old_voucher.ngay_ct;

                foreach (VoucherDetail item in vc_item.details)
                {
                    if (item.Data == null || item.Data.Count == 0)
                        continue;
                    item.Data.ForEach(x =>
                    {
                        x.stt_rec = vc_item.stt_rec;
                        x.ma_ct = old_voucher.ma_ct;
                        x.ma_cuahang = old_voucher.ma_cuahang;
                        x.ngay_ct = old_voucher.ngay_ct;

                        x.ma_ca = Startup.Shift;
                    });
                }
            }
            result_model = CommonService.checkPaid(vc_item, _PAID_PARA);
            if (!result_model.success)
            {
                return result_model;
            }

            result_model = CommonService.checkImeiUpdate(vc_item, res, this.list_imei_delete, _DETAIL_PARA);
            if (!result_model.success)
            {
                return result_model;
            }
            result_model = CommonService.checkDiscount(vc_item, _DISCOUNT_PARA);
            if (!result_model.success)
            {
                return result_model;
            }

            //return voucher object
            result_model.result = vc_item;
            return result_model;
        }

        #endregion

        #region Updated

        public CommonObjectModel Updated(object voucherItem, Query voucherQuery, int user_id)
        {
            if (voucherItem == null || voucherQuery == null)
                throw new Exception(ApiReponseMessage.Error_notExist);

            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };

            /**
             * CHECK DỮ LIỆU HỢP LỆ
             */
            if (voucherItem == null || voucherQuery == null || string.IsNullOrEmpty(voucherQuery.Prime))
            {
                model.success = false;
                model.message = "err_cast_input_to_object";
                return model;
            }
            VoucherItem vc_item = (VoucherItem)voucherItem;

            //create query
            string query = voucherQuery.Prime;

            //update prime
            string expression = vc_item.ngay_ct?.ToString("yyyyMM");
            string prime_table = this.PrimeTable.Trim() + expression;
            string update_prime_table_query = VoucherUtils.getMaterQuery(new VoucherItem(), prime_table, user_id, 2);
            query += "\n\n";
            //query += $"update {prime_table} set status = @status, ma_ca = @ma_ca, dien_giai = @dien_giai, ma_kh = @ma_kh, ma_nt = @ma_nt," +
            //    $" ty_gia = @ty_gia, t_so_luong = @t_so_luong, ma_thue = @ma_thue, t_thue = @t_thue, t_thue_nt = @t_thue_nt, t_tien = @t_tien, t_tien_nt = @t_tien_nt, t_tien2 = @t_tien2, t_tien_nt2 = @t_tien_nt2," +
            //    $" t_tt_nt = @t_tt_nt, t_tt = @t_tt, t_ck = @t_ck, t_con_no = @t_con_no, t_da_tra = @t_da_tra, tien_dat_coc = @tien_dat_coc, ma_gd = @ma_gd, loai_ct = @loai_ct, user_id2 = {user_id}, datetime2 = getdate(), ma_nvbh = @ma_nvbh, ma_nvvc = @ma_nvvc";
            //query += $" where stt_rec = @stt_rec";
            query += $"{update_prime_table_query}";

            //xóa và insert lại các bảng chi tiết
            DetailQuery? detail_query = null;
            string detail_table = "", service_table = "", discount_table = "", paid_table = "", warranty_table = "";
            if (voucherQuery.Details.Any(x => x.ParaName == _DETAIL_PARA))
            {
                //2024-08-10: bổ sung query gọi store reset trạng thái đặt hàng của các imei có trong phiếu trước khi update
                query += "\n\n";
                query += "exec Genbyte$Imei$ClearOrderStateForSaleInvoice @stt_rec, @ma_ct";

                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DETAIL_PARA);
                detail_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_DETAIL_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";

                //xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                query += $"delete from {detail_table} where stt_rec = @stt_rec \n";
                query += $"insert into {detail_table} (stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,km_yn,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv,tk_vt,so_luong,gia_nt,gia,gia_nt2,gia2,tien_nt,tien,thue,thue_nt,tt,tt_nt,xstatus,xaction,tien2,tien_nt2,cp_bh,cp_bh_nt,cp_vc,cp_vc_nt,cp_khac,cp_khac_nt,cp,cp_nt,stt_rec_ct,stt_rec0ct,gia_ban0,gia_ban_nt0,gia_ban,gia_ban_nt,tl_ck,gia_ck,gia_ck_nt,ck,ck_nt,sl_dh,sl_xuat,sl_giao,stt_rec_dh,stt_rec0dh,dh_so,dh_ln,stt_rec_px,stt_rec0px,px_so,px_ln,stt_rec_gh,stt_rec0gh,stt_rec_pn,stt_rec0pn,tk_gv,tk_dt,tk_ck,tk_cpbh,px_gia_dd,ma_nvbh_i,line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_thue,tk_thue_no,tk_thue_co,thue_suat,ma_imei,no_km_yn,ma_vt_kmtt,tien_kmqd,imei_mua, gia_vat,tl_ck09,tien_kb09,tien_max09,tien_ck09) select stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,km_yn,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv,tk_vt,so_luong,gia_nt,gia,gia_nt2,gia2,tien_nt,tien,thue,thue_nt,tt,tt_nt,xstatus,xaction,tien2,tien_nt2,cp_bh,cp_bh_nt,cp_vc,cp_vc_nt,cp_khac,cp_khac_nt,cp,cp_nt,stt_rec_ct,stt_rec0ct,gia_ban0,gia_ban_nt0,gia_ban,gia_ban_nt,tl_ck,gia_ck,gia_ck_nt,ck,ck_nt,sl_dh,sl_xuat,sl_giao,stt_rec_dh,stt_rec0dh,dh_so,dh_ln,stt_rec_px,stt_rec0px,px_so,px_ln,stt_rec_gh,stt_rec0gh,stt_rec_pn,stt_rec0pn,tk_gv,tk_dt,tk_ck,tk_cpbh,px_gia_dd,ma_nvbh_i,line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_thue,tk_thue_no,tk_thue_co,thue_suat,ma_imei,no_km_yn,ma_vt_kmtt,tien_kmqd,imei_mua, gia_vat,tl_ck09,tien_kb09,tien_max09,tien_ck09 from @{_DETAIL_PARA} \r\n";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _SERVICES_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _SERVICES_PARA);
                service_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string update_service_query = VoucherUtils.getServiceQuery(new SVServiceModel(), service_table, _SERVICES_PARA, 2);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_SERVICES_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";

                ////xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                //query += $"delete from {service_table} where stt_rec = @stt_rec \n";
                //query += $"insert into {service_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_dv, km_yn, ma_sp, ma_bp, so_lsx, dvt, he_so, ma_kho, ma_vi_tri, ma_lo, ma_vv, tk_vt, so_luong, gia_nt, gia, gia_nt2, gia2, tien_nt, tien, thue, thue_nt, tt, tt_nt, xstatus, xaction, tien2, tien_nt2, cp_bh, cp_bh_nt, cp_vc, cp_vc_nt, cp_khac, cp_khac_nt, cp, cp_nt, stt_rec_ct, stt_rec0ct, gia_ban0, gia_ban_nt0, gia_ban, gia_ban_nt, tl_ck, gia_ck, gia_ck_nt, ck, ck_nt, sl_dh, sl_xuat, sl_giao, stt_rec_dh, stt_rec0dh, dh_so, dh_ln, stt_rec_px, stt_rec0px, px_so, px_ln, stt_rec_gh, stt_rec0gh, stt_rec_pn, stt_rec0pn, tk_gv, tk_dt, tk_ck, tk_cpbh, px_gia_dd, ma_nvbh_i, ma_hd, ma_ku, ma_phi, so_dh_i, ma_td1, ma_td2, ma_td3, ma_thue, tk_thue_no, tk_thue_co, thue_suat, ma_imei) ";
                //query += $"select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_dv, km_yn, ma_sp, ma_bp, so_lsx, dvt, he_so, ma_kho, ma_vi_tri, ma_lo, ma_vv, tk_vt, so_luong, gia_nt, gia, gia_nt2, gia2, tien_nt, tien, thue, thue_nt, tt, tt_nt, xstatus, xaction, tien2, tien_nt2, cp_bh, cp_bh_nt, cp_vc, cp_vc_nt, cp_khac, cp_khac_nt, cp, cp_nt, stt_rec_ct, stt_rec0ct, gia_ban0, gia_ban_nt0, gia_ban, gia_ban_nt, tl_ck, gia_ck, gia_ck_nt, ck, ck_nt, sl_dh, sl_xuat, sl_giao, stt_rec_dh, stt_rec0dh, dh_so, dh_ln, stt_rec_px, stt_rec0px, px_so, px_ln, stt_rec_gh, stt_rec0gh, stt_rec_pn, stt_rec0pn, tk_gv, tk_dt, tk_ck, tk_cpbh, px_gia_dd, ma_nvbh_i, ma_hd, ma_ku, ma_phi, so_dh_i, ma_td1, ma_td2, ma_td3, ma_thue, tk_thue_no, tk_thue_co, thue_suat, ma_imei from @{_SERVICES_PARA}";
                query += $"{update_service_query}";
            }
            else
            {
                query += VoucherUtils.getDeleteQuery(this.ServicesTable + expression);
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _DISCOUNT_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DISCOUNT_PARA);
                discount_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string update_discount_query = VoucherUtils.getDiscountQuery(new SVDiscountModel(), discount_table, _DISCOUNT_PARA, 2);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_DISCOUNT_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";

                ////xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                //query += $"delete from {discount_table} where stt_rec = @stt_rec \n";
                //query += $"insert into {discount_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_ck, ngay_bd, ngay_kt, tl_ck, tien_ck, ma_nvbh_i, ma_vt, ma_vt_tang, sl_tang, sl_vt_tt, ma_hang_tt, tien_qd, imei_hang_mua, ma_imei) ";
                //query += $"select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_ck, ngay_bd, ngay_kt, tl_ck, tien_ck, ma_nvbh_i, ma_vt, ma_vt_tang, sl_tang, sl_vt_tt, ma_hang_tt, tien_qd, imei_hang_mua, ma_imei from @{_DISCOUNT_PARA}";
                query += $"{update_discount_query}";
            }
            else
            {
                query += VoucherUtils.getDeleteQuery(this.DiscountTable + expression);
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _PAID_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _PAID_PARA);
                paid_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string update_paid_query = VoucherUtils.getPaidQuery(new SVPaidModel(), paid_table, _PAID_PARA, 2);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_PAID_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";

                ////xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                //query += $"delete from {paid_table} where stt_rec = @stt_rec \n";
                //query += $"insert into {paid_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_thanhtoan, ma_nvbh_i, tien, tien_nt, so_the_nh, ma_chuan_chi, ma_may_pos, tk_nh_nhan, so_hd_vnpay, vi_dien_tu, so_hd_tragop, tien_phi_bh, ma_dv_tragop) ";
                //query += $"select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_thanhtoan, ma_nvbh_i, tien, tien_nt, so_the_nh, ma_chuan_chi, ma_may_pos, tk_nh_nhan, so_hd_vnpay, vi_dien_tu, so_hd_tragop, tien_phi_bh, ma_dv_tragop from @{_PAID_PARA}";
                query += $"{update_paid_query}";
            }
            else
            {
                query += VoucherUtils.getDeleteQuery(this.PaidTable + expression);
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _WARRANTY_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _WARRANTY_PARA);
                warranty_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string update_warranty_query = VoucherUtils.getWarrantyQuery(new SVWarrantyModel(), warranty_table, _WARRANTY_PARA, 2);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                //query += $"update @{_WARRANTY_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct where 1=1";
                //query += "\n\n";

                ////xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                //query += $"delete from {warranty_table} where stt_rec = @stt_rec \n";
                //query += $"insert into {warranty_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_ttbh, ma_imei, hang_sx, ma_vt) ";
                //query += $"select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_ttbh, ma_imei, hang_sx, ma_vt from @{_WARRANTY_PARA}";
                query += $"{update_warranty_query}";
            }
            else
            {
                query += VoucherUtils.getDeleteQuery(this.WarrantyTable + expression);
            }

            //insert km
            var sale_table = "r590_km$" + expression;
            string update_promotion_query = VoucherUtils.getPromotionQuery(sale_table, _DETAIL_PARA, user_id, 2);
            query += "\n";
            //query += $"delete from {sale_table} where stt_rec = @stt_rec \n";
            //query += $"insert into {sale_table} (stt_rec,stt_rec0,ma_ct,ma_dvcs,ma_kh,sl_ban,
            //
            //
            //,ngay_ct,so_ct,ma_kho,ma_vt,dvt,line_nbr,ma_imei,datetime0,datetime2,user_id0,user_id2) ";
            //query += $" select stt_rec,stt_rec0,ma_ct,@ma_dvcs,@ma_kh,so_luong,no_km_yn,ngay_ct,so_ct,ma_kho,ma_vt,dvt,line_nbr,ma_imei, getdate(), getdate(),0, 0 from @{_DETAIL_PARA} where km_yn = 1";
            query += $"{update_promotion_query}";

            query += "\n\n";
            query += "select @stt_rec as stt_rec, @ma_ct as ma_ct";

            //thực thi query update bảng prime và insert lại bảng detail có sử dụng transaction
            CoreService service = new CoreService();
            DataSet ds = service.ExecTransactionSql2DataSet(query);
            string stt_rec = ds.Tables[0].Rows[0]["stt_rec"].ToString();
            string ma_ct = ds.Tables[0].Rows[0]["ma_ct"].ToString();

            //update stt_rec cho đối tượng đang thực hiện
            vc_item.stt_rec = stt_rec;
            //up stt_rec --> chi tiết của đối tượng
            foreach (VoucherDetail vc_detail in vc_item.details)
            {
                if (vc_detail == null || vc_detail.Data == null || vc_detail.Data.Count <= 0)
                    continue;
                vc_detail.Data.ForEach(x => x.stt_rec = stt_rec);
            }

            //update các trường null
            query = $"exec fs_UpdateNullToTable '{prime_table}', '{prime_table}', 'stt_rec = ''{stt_rec}''' \n";
            query += $"exec fs_UpdateNullToTable '{detail_table}', '{detail_table}', 'stt_rec = ''{stt_rec}''' \n";
            if (!string.IsNullOrEmpty(service_table))
            {
                query += $"exec fs_UpdateNullToTable '{service_table}', '{service_table}', 'stt_rec = ''{stt_rec}''' \n";
            }
            if (!string.IsNullOrEmpty(discount_table))
            {
                query += $"exec fs_UpdateNullToTable '{discount_table}', '{discount_table}', 'stt_rec = ''{stt_rec}''' \n";
            }
            if (!string.IsNullOrEmpty(paid_table))
            {
                query += $"exec fs_UpdateNullToTable '{paid_table}', '{paid_table}', 'stt_rec = ''{stt_rec}''' \n";
            }
            if (!string.IsNullOrEmpty(warranty_table))
            {
                query += $"exec fs_UpdateNullToTable '{warranty_table}', '{warranty_table}', 'stt_rec = ''{stt_rec}''' \n";
            }
            if (!string.IsNullOrEmpty(sale_table))
            {
                query += $"exec fs_UpdateNullToTable '{sale_table}', '{sale_table}', 'stt_rec = ''{stt_rec}''' \n";
            }
            service.ExecuteNonQuery(query);

            //Update dữ liệu thanh toán và key đối với dịch vụ
            CommonService.updatePaid(vc_item, _PAID_PARA);
            CommonService.updateService(vc_item, _SERVICES_PARA, ServicesTable, vc_item.email_nhan_key);

            //insert lại dữ liệu tại bảng inquiry (i)
            string inquiry_table = this.InquiryTable.Trim() + expression;
            query = $"delete from {inquiry_table} where stt_rec = '{stt_rec}' \n";
            query += $"delete from {this.MasterTable} where stt_rec = '{stt_rec}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateInquiryTable '{this.VoucherCode}', '{inquiry_table}', '{prime_table}', '{detail_table}', 'stt_rec', '{stt_rec}', '{this.Operation}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateGrandTable '{this.VoucherCode}', '{this.MasterTable}', '{prime_table}', 'stt_rec', '{stt_rec}' \n";
            service.ExecuteNonQuery(query);

            //cập nhật lại các imei đã đặt hàng trước đó nhưng lại dùng imei khác
            string queryIMEI = "";
            if (list_imei_delete.Count > 0)
            {
                string imei = string.Join(", ", list_imei_delete);
                queryIMEI = $"exec Genbyte$IMEI$UpdateState '{vc_item.ma_cuahang}', '{imei}', '0', 0";
                service.ExecuteNonQuery(queryIMEI);
            }

            model.success = true;
            model.message = "";
            model.result = vc_item;
            return model;
        }

        #endregion

        #region Delete

        public CommonObjectModel Delete(string voucherId)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };
            CoreService service = new CoreService();

            //check sql injection
            if (!service.IsSQLInjectionValid(voucherId))
                throw new Exception(ApiReponseMessage.Error_InputData);

            //check exists & trạng thái chứng từ
            string sql = $"select * from {this.MasterTable} where status = '0' and stt_rec = @vc_id";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });
            DataSet ds = service.ExecSql2DataSet(sql, paras);
            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
                throw new Exception(ApiReponseMessage.Error_notExist);

            //Thực hiện xóa có sử dụng transaction
            DateTime ngay_ct = Convert.ToDateTime(ds.Tables[0].Rows[0]["ngay_ct"]);
            sql = $"delete from {this.MasterTable} where stt_rec = @vc_id \n";
            sql += $"delete from {this.PrimeTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.InquiryTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.DetailTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.ServicesTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.DiscountTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.PaidTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.WarrantyTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.DetailKMTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });
            service.ExecTransactionNonQuery(sql, paras);

            //return
            model.success = true;
            model.message = "";
            model.result = voucherId;
            return model;
        }

        #endregion

        #region READ
        /** 
         * Load top bản ghi của chứng từ (không phân trang) 
         */
        public CommonObjectModel TopLoading(List<Dictionary<string, object>> data)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = data
            };

            return model;
        }

        /** 
         * Lấy dữ liệu của chứng từ theo khóa chính 
         */
        public CommonObjectModel GetById(string voucherId)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(voucherId))
                throw new Exception(ApiReponseMessage.Error_InputData);

            //Lấy dữ liệu từ bảng prime và detail theo id truyền vào
            string sql = @"DECLARE @q NVARCHAR(4000), @stt_rec CHAR(13), @exp CHAR(6)
SET @stt_rec = @vc_id
IF EXISTS(SELECT 1 FROM {0} WHERE stt_rec = @stt_rec) BEGIN
	SELECT @exp = CONVERT(CHAR(6), ngay_ct, 112) FROM {0} WHERE stt_rec = @stt_rec
	SELECT @q = 'select * from {1}' + @exp + ' where stt_rec = @stt_rec '
	SELECT @q = @q + CHAR(13) + 'select a1.*, a2.ten_vt, a2.nh_vt1 from {2}' + @exp + ' a1 inner join dmvt a2 on a1.ma_vt = a2.ma_vt where stt_rec = @stt_rec'
	SELECT @q = @q + CHAR(13) + 'select d1.*, d0.ten_dv, d0.vt_ton_kho from {3}' + @exp + ' d1 inner join dmdichvu d0 on d1.ma_dv = d0.ma_dv where stt_rec = @stt_rec'
	SELECT @q = @q + CHAR(13) + 'select c1.*, c0.ten_ck, c0.loai_ck, c2.ten_loai from {4}' + @exp + ' c1 inner join dmck2 c0 on c1.ma_ck = c0.ma_ck inner join dmloaick c2 on c2.ma_loai = c0.loai_ck where stt_rec = @stt_rec'
	SELECT @q = @q + CHAR(13) + 'select t1.*,t0.ten_thanhtoan, c.ten_ctr, d.ten_vt, p.ten_pos as ten_may_pos from {5}' + @exp + ' t1 inner join dmthanhtoan t0 on t1.ma_thanhtoan = t0.ma_thanhtoan left join phctrgiamgia c on t1.ma_ctr = c.ma_ctr left join dmvt d on t1.ma_sp = d.ma_vt left join dmmaypos p on t1.ma_may_pos = p.ma_pos where stt_rec = @stt_rec'
	SELECT @q = @q + CHAR(13) + 'select b1.*, b0.ten_ttbh, b0.dia_chi, b2.ten_dv from {6}' + @exp + ' b1 left join dmtrungtambh b0 on b1.ma_ttbh = b0.ma_ttbh left join dmdichvu b2 on b1.ma_dv = b2.ma_dv where stt_rec = @stt_rec'
	EXEC sp_executesql @q, N'@stt_rec CHAR(13)', @stt_rec = @stt_rec
END";
            sql = string.Format(sql, this.MasterTable, this.PrimeTable, this.DetailTable, this.ServicesTable, this.DiscountTable, this.PaidTable, this.WarrantyTable);
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);

            //convert dataset to model
            if (ds != null && ds.Tables.Count >= 6)
            {
                VoucherItem vc_item = ds.Tables[0].ToList<VoucherItem>().FirstOrDefault(new VoucherItem());
                IList<SVDetail> pr_detail = ds.Tables[1].ToList<SVDetail>();
                IList<SVServiceModel> pr_services = ds.Tables[2].ToList<SVServiceModel>();
                IList<SVDiscountModel> pr_discount = ds.Tables[3].ToList<SVDiscountModel>();
                IList<PaidDetailBaseResponse> pr_paid = ds.Tables[4].ToList<PaidDetailBaseResponse>();
                IList<SVWarrantyModel> pr_warranty = ds.Tables[5].ToList<SVWarrantyModel>();

                pr_paid.ToList().ForEach(x => {
                    x.stt_rec_pt = APIService.EncryptForWebApp(x.stt_rec_pt, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                });
                BaseModel invoice_model = new BaseModel();
                invoice_model.masterInfo = vc_item;
                invoice_model.details = new List<DetailItemModel>();
                invoice_model.details.AddRange(new List<DetailItemModel>(){ new DetailItemModel()
                {
                    Id = 1,
                    Name = _DETAIL_PARA,
                    Data = pr_detail
                }, new DetailItemModel() {
                    Id = 2,
                    Name = _SERVICES_PARA,
                    Data = pr_services
                }, new DetailItemModel()
                {
                    Id = 3,
                    Name = _DISCOUNT_PARA,
                    Data = pr_discount
                }, new DetailItemModel()
                {
                    Id = 4,
                    Name = _PAID_PARA,
                    Data = pr_paid
                }, new DetailItemModel()
                {
                    Id = 5,
                    Name = _WARRANTY_PARA,
                    Data = pr_warranty
                }
                });

                model.result = invoice_model;
            }

            return model;
        }

        /** 
         * tìm kiếm chứng từ (có xử lý phân trang) 
         */
        public CommonObjectModel Finding(List<Dictionary<string, object>> data)
        {
            EntityCollection<VoucherFindingModel> entities = new EntityCollection<VoucherFindingModel>()
            {
                PageCount = 1,
                PageIndex = 1,
                PageSize = 50,
                RecordCount = 1,
                Items = new List<VoucherFindingModel>()
            };

            PropertyInfo[] props = typeof(VoucherFindingModel).GetProperties();
            foreach (Dictionary<string, object> record in data)
            {
                VoucherFindingModel item = new VoucherFindingModel();
                foreach (PropertyInfo property in props)
                {
                    if (record.ContainsKey(property.Name))
                    {
                        Type type = property.PropertyType;
                        if (type == typeof(int) || type == typeof(int?))
                            property.SetValue(item, Convert.ToInt32(record[property.Name]));
                        else if (type == typeof(decimal) || type == typeof(decimal?))
                            property.SetValue(item, Convert.ToDecimal(record[property.Name]));
                        else if (type == typeof(DateTime) || type == typeof(DateTime?))
                            property.SetValue(item, Convert.ToDateTime(record[property.Name]));
                        else
                            property.SetValue(item, record[property.Name]);
                    }
                }
                entities.Items.Add(item);
            }

            CommonObjectModel model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = entities
            };

            return model;
        }

        public CommonObjectModel CalculateDiscount(string ma_cuahang, string ma_kh, DateTime? ngay_lap, string hang_mua, string ma_ct = "")
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(new string[] { ma_cuahang, ma_kh, ngay_lap.GetValueOrDefault().ToString("YYYY-MM-DD") }))
                throw new Exception(ApiReponseMessage.Error_InputData);

            //Lấy dữ liệu từ bảng prime và detail theo id truyền vào
            string sql = @"declare @buy_item nvarchar(max) = @hang_mua
        exec fs_Calc$Discount$BHA @ma_cuahang, @ma_kh, @ngay_lap, @buy_item, @ma_ct";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
            new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.NVarChar,
                Value = ma_cuahang.Trim()
            },
            new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.NVarChar,
                Value = ma_kh.Trim()
            },
            new SqlParameter()
            {
                ParameterName = "@ngay_lap",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_lap
            },
            new SqlParameter()
            {
                ParameterName = "@hang_mua",
                SqlDbType = SqlDbType.NVarChar,
                Value = hang_mua.Replace("'", "''").Trim()
            },
            new SqlParameter()
            {
                ParameterName = "@ma_ct",
                SqlDbType = SqlDbType.NVarChar,
                Value = ma_ct.Trim() ?? ""
            }});
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);

            //convert dataset to model
            if (ds != null && ds.Tables.Count >= 1)
            {
                List<DiscountModel> ck_item = (List<DiscountModel>)ds.Tables[0].ToList<DiscountModel>();
                Dictionary<string, DiscountModel> DiscountDictionay = new Dictionary<string, DiscountModel>();
                foreach (var item in ck_item)
                {
                    DiscountDictionay.Add(item.ma_ck.TrimEnd(), item);
                }

                for (var i = 1; i < ds.Tables.Count; i++)
                {
                    foreach (DataRow dr in ds.Tables[i].Rows)
                    {
                        string ma_ck = dr["ma_ck"].ToString().TrimEnd();
                        Dictionary<string, object> dicRow = new Dictionary<string, object>();
                        foreach (DataColumn col in dr.Table.Columns)
                        {
                            dicRow.Add(col.ColumnName, dr[col]);
                        }
                        if (DiscountDictionay[ma_ck].items == null)
                        {
                            List<object> list = new List<object>();
                            list.Add(dicRow);
                            DiscountDictionay[ma_ck].items = list;
                        }
                        else
                        {
                            List<object> list = (List<object>)DiscountDictionay[ma_ck].items;
                            list.Add(dicRow);
                            DiscountDictionay[ma_ck].items = list;
                        }
                    }
                }
                model.result = ck_item;
            }

            return model;
        }

        /** 
        * Lấy danh sách dữ liệu của danh mục có xử lý phân trang
        * entities: input object có type là EntityCollection<T>
        */
        public CommonObjectModel GetByPaging(object entities, string order_by = "", int page_index = 1, int page_size = 0)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            //code here...

            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = entities
            };
            return result;
        }

        /** 
       * Lấy dữ liệu khác của từng mã
       * entities: input object có type là EntityCollection<T>
       */
        public CommonObjectModel GetOtherData(string so_ct, string ma_cuahang)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            //code here...

            CommonObjectModel result = new CommonObjectModel()
            {
                message = "",
                success = true,
                result = null
            };
            return result;
        }
        #endregion
        public CommonObjectModel checkPaid(VoucherItem vc_item)
        {
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            if (vc_item.status != "2") return result_model;
            List<PaidDetailBase> paidDetails = new List<PaidDetailBase>();
            var ma_cuahang = "";
            if (vc_item.details.Any(x => x.Id == 4))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 4);

                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var paid = item as SVPaidModel;
                        if (paid != null && !string.IsNullOrEmpty(paid.ma_thanhtoan))
                        {
                            paidDetails.Add(paid);
                        }
                    }

                }
            }
            if (paidDetails != null)
            {
                if (paidDetails.Find(x => x.ma_thanhtoan.Trim() == "MAGG") != null)
                {
                    result_model = CommonService.checkDicountCode(paidDetails.Find(x => x.ma_thanhtoan.Trim() == "MAGG"));
                }
            }
            return result_model;
        }
        public void updatePaid(VoucherItem vc_item)
        {
            if (vc_item.status == "2")
            {
                if(vc_item.details.FirstOrDefault(x => x.Name == _PAID_PARA) != null)
                {
                    VoucherDetail? item_model = vc_item.details.FirstOrDefault(x => x.Name == _PAID_PARA);
                    if (item_model.Data == null) return;
                    List<SVPaidModel>? detail_list = new List<SVPaidModel>();
                    foreach (var item in item_model.Data)
                    {
                        if (item is SVPaidModel sVPaid)
                        {
                            detail_list.Add(sVPaid);
                        }
                    }
                    // Điểm quy đổi
                    CommonService.postConversionPoint(vc_item, detail_list.FirstOrDefault(x => x.ma_thanhtoan == "DIEMQD"));
                    // Active mã giảm giá
                    if (detail_list.FirstOrDefault(x => x.ma_thanhtoan == "MAGG") != null)
                    {
                        CommonService.updateDiscountCode(vc_item.stt_rec, vc_item.so_ct, vc_item.ngay_ct, vc_item.ma_kh, detail_list.FirstOrDefault(x => x.ma_thanhtoan == "MAGG"));
                    }
                }
                else
                {
                    // Điểm quy đổi
                    CommonService.postConversionPoint(vc_item, null);
                }
            }
        }
        public void updateService(VoucherItem vc_item)
        {
            if (vc_item.status == "2" && vc_item.details.FirstOrDefault(x => x.Name == _SERVICES_PARA) != null)
            {
                VoucherDetail? item_model = vc_item.details.FirstOrDefault(x => x.Name == _SERVICES_PARA);
                if (item_model.Data == null) return;
                List<ServiceDetailBase>? service_list = new List<ServiceDetailBase>();
                foreach (var item in item_model.Data)
                {
                    if (item is ServiceDetailBase service_base)
                    {
                        service_list.Add(service_base);
                    }
                }
                // GET list key from service
                bool flag = true;
                List<KeyServiceModel> list_key = CommonService.getKeys(service_list);
                if (list_key != null && list_key.Count > 0)
                {
                    string expression = vc_item.ngay_ct?.ToString("yyyyMM");
                    // Update table service detail 
                    flag = CommonService.updateServiceDetailTable(this.ServicesTable + expression, vc_item.stt_rec, list_key);
                    if (flag)
                    {
                        // UPdate active key
                        CommonService.updateStatusKey(vc_item.stt_rec, vc_item.so_ct, vc_item.ngay_ct, vc_item.ma_kh, vc_item.email_nhan_key, list_key);
                    }
                }
            }
        }
        CommonObjectModel checkImeiInsert(VoucherItem vc_item)
        {
            var listImei = new List<string>();
            var ma_cuahang = "";
            if (vc_item.details.Any(x => x.Id == 1))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as SVDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            listImei.Add(svDetail.ma_imei.Trim());
                            ma_cuahang = svDetail.ma_cuahang;
                        }
                    }

                }
            }
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            var imeiService = new Imei.Service();
            List<Imei.ImeiState> state_imei = imeiService.GetStateOfImeis(listImei);
            List<string> exists = state_imei.Where(x => x.exists_yn == false).Select(x => x.ma_imei).ToList();
            List<string> dat_hang = state_imei.Where(x => x.dat_hang_yn == true).Select(x => x.ma_imei).ToList();
            if (exists != null && exists.Count > 0)
            {
                var list_result_error = new List<ResultMessageError>();
                list_result_error.Add(new ResultMessageError
                {
                    name = "%imei",
                    value = string.Join(", ", exists)
                });
                result_model.success = false;
                result_model.message = "imei_not_exists";
                result_model.result = list_result_error;
            }
            if (dat_hang != null && dat_hang.Count > 0)
            {
                var list_result_error = new List<ResultMessageError>();
                list_result_error.Add(new ResultMessageError
                {
                    name = "%imei",
                    value = string.Join(", ", dat_hang)
                });
                result_model.success = false;
                result_model.message = "dat_hang_yn_yes";
                result_model.result = list_result_error;
            }
            return result_model;
        }
        CommonObjectModel checkImeiUpdate(VoucherItem vc_item, BaseModel vc_item_old)
        {
            var listImei = new List<string>();
            var ma_cuahang = "";
            if (vc_item.details.Any(x => x.Id == 1))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as SVDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            listImei.Add(svDetail.ma_imei.Trim());
                            ma_cuahang = svDetail.ma_cuahang;
                        }
                    }

                }
            }

            var listImei_old = new List<string>();
            var ma_cuahang_old = "";
            if (vc_item_old.details.Any(x => x.Id == 1))
            {
                DetailItemModel? item_detail = vc_item_old.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {
                    
                    foreach (var item in item_detail.Data as List<SVDetail>)
                    {
                        var svDetail = item as SVDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            listImei_old.Add(svDetail.ma_imei.Trim());
                            ma_cuahang_old = svDetail.ma_cuahang;
                        }
                    }

                }
            }

            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            var imeiService = new Imei.Service();
            List<Imei.ImeiState> state_imei = imeiService.GetStateOfImeis(listImei);
            List<string> exists = state_imei.Where(x => x.exists_yn == false).Select(x => x.ma_imei).ToList();
            List<string> dat_hang = state_imei.Where(x => x.dat_hang_yn == true).Select(x => x.ma_imei).ToList();
            if (exists != null && exists.Count > 0)
            {
                var list_result_error = new List<ResultMessageError>();
                list_result_error.Add(new ResultMessageError
                {
                    name = "%imei",
                    value = string.Join(", ", exists)
                });
                result_model.success = false;
                result_model.message = "imei_not_exists";
                result_model.result = list_result_error;
            }
            listImei_old.Except(listImei).ToList().ForEach(x =>
            {
                list_imei_delete.Add(x);
            });
            dat_hang = dat_hang.Except(listImei_old).ToList() ;
            if (dat_hang != null && dat_hang.Count > 0)
            {
                var list_result_error = new List<ResultMessageError>();
                list_result_error.Add(new ResultMessageError
                {
                    name = "%imei",
                    value = string.Join(", ", dat_hang)
                });
                result_model.success = false;
                result_model.message = "dat_hang_yn_yes";
                result_model.result = list_result_error;
            }
            return result_model;
        }
        public List<ImeiState> GetImeis(CommonObjectModel model)
        {
            return new List<ImeiState>();
            VoucherItem vc_item = (VoucherItem)model.result;
            var listImei = new List<string>();
            var ma_cuahang = "";
            if (vc_item.details.Any(x => x.Id == 1))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as SVDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            listImei.Add(svDetail.ma_imei.Trim());
                            ma_cuahang = svDetail.ma_cuahang;
                        }
                    }

                }
            }
            CoreService service = new CoreService();
            //call imei
            var query = $"exec Genbyte$IMEI$CheckStateBeforeSave '{string.Join(',', listImei)}', '{ma_cuahang}', '2' \n";
            var dataSet = service.ExecSql2DataSet(query);
            if (dataSet.Tables.Count > 0)
            {
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    var listImeiInvalid = dataSet.Tables[0].ToList<ImeiState>().Where(a => a.state == false).ToList();
                    return listImeiInvalid;
                }
            }

            return new List<ImeiState>();
        }

        CommonObjectModel checkDiscount(VoucherItem vc_item)
        {
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            if (vc_item.details.Any(x => x.Name == _DISCOUNT_PARA))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Name == _DISCOUNT_PARA);
                bool flag = false;
                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as SVDiscountModel;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_ck) && svDetail.loai_ck == "04")
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if(flag)
                {
                    if (string.IsNullOrEmpty(vc_item.nguoi_duyet_ck))
                    {
                        result_model.success = false;
                        result_model.message = "lbl_invalid_ck04";
                    }
                    else
                    {
                        bool check = new Employee.Service().checkUser(vc_item.nguoi_duyet_ck, "BGD");
                        if (!check)
                        {
                            result_model.success = false;
                            result_model.message = "lbl_invalid_ck04";
                        }
                    }
                }
            }
            return result_model;
        }


        public bool validMoneyMerchandise(
            List<SVDetail> list_hang_hoa,
            List<SVServiceModel> list_dich_vu,
            List<SVWarrantyModel> list_goi_cuoc,
            VoucherItem vc_item)
        {
            var totalHH = list_hang_hoa.Sum(x => x.tt);
            var totalDV = list_dich_vu.Sum(x => x.tt);
            var totalGC = list_goi_cuoc
                .Where(e => e.naptien_hh_yn)
                .Sum(x => x.tt);
            var total = totalHH + totalDV + totalGC;

            return total != vc_item.t_tt;
        }

        public bool validTotalPayment(VoucherItem vc_item)
        {
            var totalPayment = vc_item.fqty1;
            var tienDaTra = vc_item.t_da_tra;
            var tienConNo = vc_item.t_con_no;

            return totalPayment != (tienConNo + tienDaTra);
        }

        private void CheckKhuyenMai(List<SVDetail> detailList)
        {
            detailList.ForEach(x =>
            {
                if (x.km_yn == 1 && !string.IsNullOrEmpty(x.ma_imei))
                {
                    x.no_km_yn = false;
                }
            });
        }

    }
}