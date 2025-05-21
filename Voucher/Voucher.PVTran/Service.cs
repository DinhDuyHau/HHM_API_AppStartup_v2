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
using Voucher.PVTran.Model;
using Microsoft.VisualBasic;
using Genbyte.Component.Voucher.Model;
using Genbyte.Base.Security;
using Microsoft.Extensions.Configuration;

namespace Voucher.PVTran
{
    public class Service : IVoucherService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "PNA";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "c571$000000";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "m571$";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "i571$";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "d571$";
        private const string _DETAIL_PARA = "d571";

        public string TaxTable { get; } = "m571ext$";
        private const string _TAX_PARA = "m571ext";

        // bảng lưu dữ liệu chi tiết của chiết khấu
        public string DiscountTable { get; } = "d571ck$";
        private const string _DISCOUNT_PARA = "d571ck";

        // Lấy danh sách imei xóa khỏi grid
        List<ImeiItem> list_imei_delete = new List<ImeiItem>();

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "ma_kh,ma_dvcs,ma_cuahang,ma_ca;#10$,#20$,#30$, #40$; , , , :ma_kho,ma_vt,ma_imei;#10$,#20$,#30$;d571,d571,d571";
        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '000000', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(ma_ca) as ma_ca,ngay_ct,rtrim(so_ct) as so_ct,rtrim(so_ct0) as so_ct0,rtrim(ma_gd) as ma_gd,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang,rtrim(Dien_giai) as Dien_giai,t_so_luong, t_tien_nt,t_thue_nt,t_tt_nt,rtrim(ma_nt) as ma_nt,rtrim(ma_ct) as ma_ct,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(a.ma_ca) as ma_ca,c.ten_ca, rtrim(a.ma_cuahang) as ma_cuahang, ngay_ct,rtrim(so_ct) as so_ct,rtrim(so_ct0) as so_ct0,rtrim(ma_gd) as ma_gd,rtrim(a.ma_kh) as ma_kh,b.ten_kh,rtrim(a.dien_giai) as dien_giai, t_so_luong, t_tien_nt,t_thue_nt,t_tt_nt,rtrim(ma_nt) as ma_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.comment,z.comment2,'''' as Hash', 'a left join dmkh b on a.ma_kh = b.ma_kh left join dmca c on a.ma_ca = c.ma_ca left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id where ma_gd = ''1'' ', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "''', '@@SYSID'";
        public string WhereClauseFinding { get; } = "ma_gd = '1'";
        /// <summary>
        /// Khai báo các hành động của user tác động đến service hiện tại: addnew, edit, read, delete
        /// </summary>
        public UserAction Action { get; set; }

        /// <summary>
        /// Khai báo quyền truy cập cho các xử lý CRUD
        /// </summary>
        public AccessRight VoucherRight { get; set; }
        private readonly IConfiguration _configuration;

        public Service(IConfiguration configuration, string sysid)
        {
            Authoriztion authoriztion = CommonService.GetAuthoriztion(sysid);
            VoucherRight = new AccessRight();
            VoucherRight.AllowReadAll = authoriztion.view_yn;
            VoucherRight.AllowRead = authoriztion.access_yn;
            VoucherRight.AllowCreate = authoriztion.add_yn;
            VoucherRight.AllowUpdate = authoriztion.edit_yn;
            VoucherRight.AllowDelete = authoriztion.del_yn;
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

            //Cập nhật ngày chứng từ là ngày hiện thời của Server
            vc_item.ngay_ct = DateTime.Today;

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: PVDetail
            int index_value = 1;
            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<PVDetail>? detail_list = JsonSerializer.Deserialize<List<PVDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        //cập nhật ngày chứng từ
                        detail_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(PVDetail).Name;
                }
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
            query += "\n\n";
            query += $"insert into {prime_table} (stt_rec, ma_ct, so_ct, ngay_ct, status, ma_dvcs, ma_cuahang, ma_ca, dien_giai, user_id0, user_id2, datetime0, datetime2)";
            query += $" select @stt_rec, @ma_ct, @so_ct, @ngay_ct, @status, @ma_dvcs, @ma_cuahang, @ma_ca, @dien_giai, {user_id}, {user_id}, getdate(), getdate() ";

            //insert các bảng chi tiết
            DetailQuery? detail_query = null;
            string detail_table = "";
            string tax_table = "";
            if (voucherQuery.Details.Any(x => x.ParaName == _DETAIL_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DETAIL_PARA);
                detail_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_DETAIL_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";
                query += $"insert into {detail_table} (stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, so_luong) select stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, so_luong from @{_DETAIL_PARA}";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _TAX_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _TAX_PARA);
                tax_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_TAX_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";
                query += $"insert into {tax_table} (stt_rec, stt_rec0, ma_dvcs, loai_ct, ma_ct, ngay_lct, ngay_ct, so_ct, ngay_ct0, so_ct0, so_seri0, mau_bc, ma_tc, ma_kh, ten_kh, dia_chi, ma_so_thue, ma_kh2, ten_vt, so_luong, ty_gia, ma_nt, gia_nt, gia, t_tien_nt, t_tien, ma_thue, thue_suat, t_thue_nt, t_thue, ma_tt, tk_thue_no, tk_du, ma_kho, ma_vv, ma_sp, ma_bp, so_lsx, ghi_chu, nam, ky, line_nbr, status, datetime0, datetime2, user_id0, user_id2, ma_hd, ma_ku, ma_phi, so_dh, ma_td1, ma_td2, ma_td3, sl_td1, sl_td2, sl_td3, ngay_td1, ngay_td2, ngay_td3, gc_td1, gc_td2, gc_td3, s1, ma_ca, ma_cuahang, s4, s5, s6, s7, s8, s9, ma_mau_ct) select stt_rec, stt_rec0, ma_dvcs, loai_ct, ma_ct, ngay_lct, ngay_ct, so_ct, ngay_ct0, so_ct0, so_seri0, mau_bc, ma_tc, ma_kh, ten_kh, dia_chi, ma_so_thue, ma_kh2, ten_vt, so_luong, ty_gia, ma_nt, gia_nt, gia, t_tien_nt, t_tien_nt, ma_thue, thue_suat, t_thue_nt, t_thue_nt, ma_tt, tk_thue_no, tk_du, ma_kho, ma_vv, ma_sp, ma_bp, so_lsx, ghi_chu, nam, ky, line_nbr, status, datetime0, datetime2, user_id0, user_id2, ma_hd, ma_ku, ma_phi, so_dh, ma_td1, ma_td2, ma_td3, sl_td1, sl_td2, sl_td3, ngay_td1, ngay_td2, ngay_td3, gc_td1, gc_td2, gc_td3, s1, ma_ca, ma_cuahang, s4, s5, s6, s7, s8, s9, ma_mau_ct from @{_TAX_PARA}";
            }

            query += "\n\n";
            query += "select @stt_rec as stt_rec";

            //thực thi query insert vào bảng prime và detail có sử dụng transaction
            CoreService service = new CoreService();
            DataSet ds = service.ExecTransactionSql2DataSet(query);
            string stt_rec = ds.Tables[0].Rows[0]["stt_rec"].ToString();

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
            service.ExecuteNonQuery(query);

            //insert bảng master (c) & inquiry (i)
            string inquiry_table = this.InquiryTable.Trim() + expression;
            query = $"exec MokaOnline$App$Voucher$UpdateInquiryTable '{this.VoucherCode}', '{inquiry_table}', '{prime_table}', '{detail_table}', 'stt_rec', '{stt_rec}', '{this.Operation}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateGrandTable '{this.VoucherCode}', '{this.MasterTable}', '{prime_table}', 'stt_rec', '{stt_rec}'";

            service.ExecuteNonQuery(query);

            model.success = true;
            model.message = "create_voucher_success";
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
            if (vc_item == null) return null;
            vc_item.ma_ct = this.VoucherCode;
            vc_item.ngay_lct = vc_item.ngay_ct;

            //Danh sách IMEI
            List<string> list_imei = new List<string>();

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: PVDetail
            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: SVDetail
            int index_value = 1;
            string ma_thue = "";
            decimal thue_suat = 0;
            if (data.details == null)
            {
                return null;
            }

            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<PVDetail>? detail_list = JsonSerializer.Deserialize<List<PVDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(PVDetail).Name;
                    foreach (var item in detail_list)
                    {
                        List<string> imei = item.ma_imei.Split(',').ToList();
                        foreach (var imei_item in imei)
                        {
                            list_imei.Add(imei_item.Trim());
                        }
                    }
                }
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
                                List<PVDetail>? detail_list = JsonSerializer.Deserialize<List<PVDetail>>((JsonElement)item_model.Data);
                                if (detail_list != null && detail_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    detail_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(detail_list);
                                }
                                item_detail.Detail_Type = typeof(PVDetail).Name;
                                ma_thue = detail_list.Max(x => x.ma_thue);
                                thue_suat = detail_list.Max(x => x.thue_suat);
                                foreach (var detail_item in detail_list)
                                {
                                    // kiểm tra hợp lệ kho với cửa hàng
                                    if(IsInvalidWarehouse(detail_item.ma_cuahang, detail_item.ma_kho))
                                    {
                                        result_model.success = false;
                                        result_model.message = "invalid_warehouse";
                                        return result_model;
                                    }

                                    List<string> imei = detail_item.ma_imei.Split(',').ToList();
                                    foreach (var imei_item in imei)
                                    {
                                        list_imei.Add(imei_item.Trim());
                                    }
                                }
                                break;
                            case 2:
                                List<TaxDetail>? tax_list = JsonSerializer.Deserialize<List<TaxDetail>>((JsonElement)item_model.Data);
                                if (tax_list != null && tax_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    tax_list.ForEach((item) =>
                                    {
                                        item.ngay_ct = vc_item.ngay_ct;
                                        item.ma_cuahang = vc_item.ma_cuahang;
                                        item.ma_dvcs = vc_item.ma_dvcs;
                                        item.ma_ca = vc_item.ma_ca;
                                        item.ngay_lct = vc_item.ngay_lct;
                                        item.loai_ct = vc_item.loai_ct;
                                        item.ma_nt = vc_item.ma_nt;
                                        item.mau_bc = "3";
                                        item.ma_tc = "1";
                                        item.ma_kh = vc_item.ma_kh;
                                        item.ma_thue = ma_thue;
                                        item.thue_suat = thue_suat;
                                        item.t_thue_nt = vc_item.t_thue_nt;
                                        item.t_thue = vc_item.t_thue_nt;
                                        item.t_tien_nt = vc_item.t_tien_nt;
                                        item.t_tien = vc_item.t_tien_nt;
                                    });
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(tax_list);
                                }
                                item_detail.Detail_Type = typeof(TaxDetail).Name;
                                break;
                            case 3:
                                List<SVDiscountModel>? discount_list = JsonSerializer.Deserialize<List<SVDiscountModel>>((JsonElement)item_model.Data);
                                if (discount_list != null && discount_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    discount_list.ForEach(x => {
                                        x.ngay_ct = vc_item.ngay_ct;
                                        x.stt_rec = APIService.DecryptForWebApp(x.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                                    });

                                    foreach(var discount in discount_list)
                                    {
                                        // diễn giải ko được để trống
                                        if(string.IsNullOrEmpty(discount.dien_giai))
                                        {
                                            result_model.success = false;
                                            result_model.message = "invalid_diengiai_discount_ncc";
                                            return result_model;
                                        }
                                        // tien ko được = 0
                                        if (discount.tien == 0)
                                        {
                                            result_model.success = false;
                                            result_model.message = "invalid_tien_discount_ncc";
                                            return result_model;
                                        }
                                    }
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(discount_list);
                                }
                                item_detail.Detail_Type = typeof(SVDiscountModel).Name;
                                break;
                            default:
                                break;
                        }

                    }
                }
                index_value++;
            }

            // Lấy danh sách tất cả các imei
            List<string> imeis = new List<string>();
            VoucherDetail? item_detail2 = vc_item.details.FirstOrDefault(x => x.Id == index_value);
            if (item_detail2 != null) foreach (PVDetail row_entity in item_detail2.Data)
                {
                    if (!string.IsNullOrWhiteSpace(row_entity.ma_imei))
                    {
                        imeis.AddRange(row_entity.ma_imei.Split(",").ToList().Select(x => x.Trim()));
                    }
                }

            //kiểm tra trùng imei từ danh sách nhập vào chi tiết chứng từ
            if (imeis != null && imeis.Count > 0)
            {
                IEnumerable<string> duplicate_imeis = imeis.GroupBy(x => x.ToUpper())
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);

                if (duplicate_imeis != null && duplicate_imeis.Count() > 0)
                {
                    result_model.success = false;
                    result_model.message = "err_imei_duplicate";
                    return result_model;
                }
            }

            // Kiểm tra ngay_ct

            if (vc_item.ngay_ct > DateTime.Now)
            {
                result_model.success = false;
                result_model.message = "date_err";
                return result_model;
            }

            //Check tồn tại chứng từ & trạng thái chứng từ thuộc danh sách trạng thái được phép sửa
            string sql = @"DECLARE @check TABLE (
	is_success BIT,
	message VARCHAR(100)
)
DECLARE @status_older CHAR(1)
INSERT INTO @check (is_success, message) VALUES (1, '')
SELECT @status_older = (SELECT status FROM " + this.MasterTable + @" WHERE stt_rec = @vc_id)
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

            /**
             * Lấy thông tin chứng từ cũ trước khi thực hiện update
             */

            /* 2024-08-15: comment by tuananhnl
             * code của sonnnt => không hiểu để làm gì ở đoạn này
             * -------
            sql = "EXEC Genbyte$System$GetVoucherPrimeInfo @vc_id, @vc_code";
            paras = new List<SqlParameter>();
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
            #endregion

            * ---------
            * 2024-08-15: end
            */

            BaseModel res = (BaseModel)this.GetById(vc_item.stt_rec.Replace("'", "''")).result;
            VoucherItem old_voucher = (VoucherItem)res.masterInfo;
            if (old_voucher != null)
            {
                //Gán mã ca theo thông tin đăng nhập
                vc_item.ma_ca = Startup.Shift;

                //Gán lại các thông tin từ chứng từ cũ trước khi sửa: ma_dvcs, ma_cuahang, ma_ca, ngay_ct
                //(không cho sửa các trường này)
                vc_item.ma_dvcs = old_voucher.ma_dvcs;
                vc_item.ma_cuahang = old_voucher.ma_cuahang;
                vc_item.ngay_ct = old_voucher.ngay_ct;

                foreach (VoucherDetail item in vc_item.details)
                {
                    if(item.Data != null)
                    {
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
            }
            result_model = checkImeiUpdate(vc_item, res);
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
            query += "\n\n";
            query += $"update {prime_table} set ma_ca = @ma_ca, ma_tt = @ma_tt, ma_gd = @ma_gd, ma_kh = @ma_kh, dien_giai = @dien_giai, status = @status, user_id2 = {user_id}, datetime2 = getdate(), t_ck = @t_ck_nt, t_ck_nt = @t_ck_nt, t_tt = @t_tt_nt, t_tt_nt = @t_tt_nt, s4 = @s4, t_tien_nt = @t_tien_nt, t_tien = @t_tien_nt, t_thue_nt = @t_thue_nt, t_thue = @t_thue_nt";
            query += $" where stt_rec = @stt_rec";

            //xóa và insert lại các bảng chi tiết
            DetailQuery? detail_query = null;
            string detail_table = "";
            string tax_table = "";
            string discount_table = "";
            if (voucherQuery.Details.Any(x => x.ParaName == _DETAIL_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DETAIL_PARA);
                detail_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_DETAIL_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";

                query += $"\ndelete from {detail_table} where stt_rec = @stt_rec \n";
                // sửa lại dữ liệu imei và một số các dữ liệu khác
                query += $"insert into {detail_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, ma_vt, dvt, he_so, ma_kho, so_luong, gia_nt, gia, gia_nt0, gia0, tien_nt, tien, ma_thue, thue_suat, thue, thue_nt, tt, tt_nt, tien0, tien_nt0, line_nbr, so_dh_i, ma_ca, ma_cuahang, ma_imei, budslive, ma_td1, ck, ck_nt, thue_ck, thue_ck_nt, s4, gc_td1) select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, ma_vt, dvt, he_so, ma_kho, so_luong, gia_nt, gia, gia_nt0, gia0, tien_nt, tien, ma_thue, thue_suat, thue, thue_nt, tt, tt_nt, tien0, tien_nt0, line_nbr, so_dh_i, ma_ca, ma_cuahang, ma_imei, budslive, ma_td1, ck, ck_nt, thue_ck, thue_ck_nt, s4, gc_td1 from @{_DETAIL_PARA} \r\n";

                //xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                //query += $"delete from {detail_table} where stt_rec = @stt_rec \n";
                //query += $"insert into {detail_table} (stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, dvt, ma_imei, ma_kho, so_luong, gia_nt, tien_nt) ";
                //query += $"select stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, dvt, ma_imei, ma_kho, so_luong, gia_nt, tien_nt from @{_DETAIL_PARA}"; 
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _TAX_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _TAX_PARA);
                tax_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_TAX_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";

                //xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                query += $"delete from {tax_table} where stt_rec = @stt_rec \n";
                query += $"insert into {tax_table} (stt_rec, stt_rec0, ma_dvcs, loai_ct, ma_ct, ngay_lct, ngay_ct, so_ct, ngay_ct0, so_ct0, so_seri0, mau_bc, ma_tc, ma_kh, ten_kh, dia_chi, ma_so_thue, ma_kh2, ten_vt, so_luong, ty_gia, ma_nt, gia_nt, gia, t_tien_nt, t_tien, ma_thue, thue_suat, t_thue_nt, t_thue, ma_tt, tk_thue_no, tk_du, ma_kho, ma_vv, ma_sp, ma_bp, so_lsx, ghi_chu, nam, ky, line_nbr, status, datetime0, datetime2, user_id0, user_id2, ma_hd, ma_ku, ma_phi, so_dh, ma_td1, ma_td2, ma_td3, sl_td1, sl_td2, sl_td3, ngay_td1, ngay_td2, ngay_td3, gc_td1, gc_td2, gc_td3, s1, ma_ca, ma_cuahang, s4, s5, s6, s7, s8, s9, ma_mau_ct) select stt_rec, stt_rec0, ma_dvcs, loai_ct, ma_ct, ngay_lct, ngay_ct, so_ct, ngay_ct0, so_ct0, so_seri0, mau_bc, ma_tc, ma_kh, ten_kh, dia_chi, ma_so_thue, ma_kh2, ten_vt, so_luong, ty_gia, ma_nt, gia_nt, gia, t_tien_nt, t_tien_nt, ma_thue, thue_suat, t_thue_nt, t_thue_nt, ma_tt, tk_thue_no, tk_du, ma_kho, ma_vv, ma_sp, ma_bp, so_lsx, ghi_chu, nam, ky, line_nbr, status, datetime0, datetime2, user_id0, user_id2, ma_hd, ma_ku, ma_phi, so_dh, ma_td1, ma_td2, ma_td3, sl_td1, sl_td2, sl_td3, ngay_td1, ngay_td2, ngay_td3, gc_td1, gc_td2, gc_td3, s1, ma_ca, ma_cuahang, s4, s5, s6, s7, s8, s9, ma_mau_ct from @{_TAX_PARA}";
            }
            if (voucherQuery.Details.Any(x => x.ParaName == _DISCOUNT_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DISCOUNT_PARA);
                discount_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");
                string update_discount_query = VoucherUtils.getDiscountQuery(new SVDiscountModel(), discount_table, _DISCOUNT_PARA, 2);

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"{update_discount_query}";
            }
            else
            {
                query += VoucherUtils.getDeleteQuery(this.DiscountTable + expression);
            }
            query += "\n\n";

            //2024-07-01: begin
            //cập nhật ngày chứng từ tự động lấy mặc định là ngày hệ thống
            DateTime new_ngay_ct = DateTime.Today;
            string new_partition = new_ngay_ct.ToString("yyyyMM");
            string new_prime_table = this.PrimeTable.Trim() + new_partition;
            string new_detail_table = this.DetailTable.Trim() + new_partition;
            string new_inquiry_table = this.InquiryTable.Trim() + new_partition;
            string new_tax_table = this.TaxTable.Trim() + new_partition;

            string old_prime_table = prime_table;
            string old_detail_table = detail_table;
            string old_tax_table = tax_table;
            string old_inquiry_table = this.InquiryTable.Trim() + expression;

            query += "\n";
            query += $"declare @today DATETIME = '{new_ngay_ct.ToString("yyyy-MM-dd")}' \n";
            query += $"declare @old_partition char(6) = '{expression}', @new_partition char(6) = '{new_partition}' \n";
            query += @$"if exists(select 1 from {old_prime_table} where stt_rec = @stt_rec and ngay_ct <> @today) begin
	SET XACT_ABORT ON
	BEGIN TRAN
	BEGIN TRY
		if @old_partition <> @new_partition begin
			select * into #tmp_prime from {old_prime_table} where stt_rec = @stt_rec
			select * into #tmp_detail from {old_detail_table} where stt_rec = @stt_rec
			select * into #tmp_tax from {old_tax_table} where stt_rec = @stt_rec

			update #tmp_prime set ngay_ct = @today where stt_rec = @stt_rec
			update #tmp_detail set ngay_ct = @today where stt_rec = @stt_rec
			update #tmp_tax set ngay_ct = @today where stt_rec = @stt_rec

			delete from {old_prime_table} where stt_rec = @stt_rec
			delete from {old_detail_table} where stt_rec = @stt_rec
			delete from {old_tax_table} where stt_rec = @stt_rec
			delete from {old_inquiry_table} where stt_rec = @stt_rec

            update {this.MasterTable} set ngay_ct = @today where stt_rec = @stt_rec
			insert into {new_prime_table} select * from #tmp_prime
			insert into {new_detail_table} select * from #tmp_detail
			insert into {new_tax_table} select * from #tmp_tax

			drop table #tmp_prime
			drop table #tmp_detail
			drop table #tmp_tax
		end
		else begin
			update {this.MasterTable} set ngay_ct = @today where stt_rec = @stt_rec
			update {old_prime_table} set ngay_ct = @today where stt_rec = @stt_rec
			update {old_detail_table} set ngay_ct = @today where stt_rec = @stt_rec
			update {old_tax_table} set ngay_ct = @today where stt_rec = @stt_rec
			update {old_inquiry_table} set ngay_ct = @today where stt_rec = @stt_rec
		end
		COMMIT
	END TRY
	BEGIN CATCH
	   ROLLBACK
	   DECLARE @ErrorMessage VARCHAR(2000)
	   SELECT @ErrorMessage = ERROR_MESSAGE()
	   INSERT INTO log_syncerror (name, cr_date, message) VALUES('PVTran', GETDATE(), @ErrorMessage)
	   RAISERROR(@ErrorMessage, 16, 1)
	END CATCH
	SET XACT_ABORT OFF
end";
            query += "\n\n";

            if (expression != new_partition)
            {
                //set lại tên bảng theo phân kỳ mới
                prime_table = new_prime_table;
                detail_table = new_detail_table;
                tax_table = new_tax_table;
                expression = new_partition;
            }
            //2024-07-01: end

            query += "select @stt_rec as stt_rec";

            //thực thi query update bảng prime và insert lại bảng detail có sử dụng transaction
            CoreService service = new CoreService();
            DataSet ds = service.ExecTransactionSql2DataSet(query);
            string stt_rec = ds.Tables[0].Rows[0]["stt_rec"].ToString();

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
            service.ExecuteNonQuery(query);

            //insert lại dữ liệu tại bảng inquiry (i)
            string inquiry_table = this.InquiryTable.Trim() + expression;
            query = $"delete from {inquiry_table} where stt_rec = '{stt_rec}' \n";
            query += $"delete from {this.MasterTable} where stt_rec = '{stt_rec}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateInquiryTable '{this.VoucherCode}', '{inquiry_table}', '{prime_table}', '{detail_table}', 'stt_rec', '{stt_rec}', '{this.Operation}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateGrandTable '{this.VoucherCode}', '{this.MasterTable}', '{prime_table}', 'stt_rec', '{stt_rec}' \n";

            service.ExecuteNonQuery(query);

            // cập nhật lại phiếu DO bản QT nếu có sửa giá
            string queryUpdateDO = "";
            queryUpdateDO = $"exec rs_Update$Voucher$Data$DOTran '{stt_rec}', 'DO1' \n";
            service.ExecuteNonQuery(queryUpdateDO, null, ConnectType.Accounting);

            //Nếu trạng thái là hoàn thành thì đẩy vào imei vào hệ thống
            string queryIMEI = "";
            if (vc_item.status == "2")
            {
                //create query insert IMEI
                queryIMEI = $"exec Genbyte$IMEI$Purchase$Post '{stt_rec}', '{vc_item.ma_ct}'";
                DataSet ds_post = service.ExecSql2DataSet(queryIMEI);
                if(ds_post != null && ds_post.Tables.Count > 0 && ds_post.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds_post.Tables[0].Rows[0];
                    model.success = Convert.ToBoolean(dr["is_success"]);
                    model.message = dr["msg"].ToString();
                    //model.result = new List<VoucherItem>() { vc_item };
                    model.result = vc_item;

                    return model;
                }
            }
            else
            {
                List<ImeiItem> list_imei = new List<ImeiItem>();
                // id = 1 ==> type: PVDetail
                int index_value = 1;
                if (vc_item.details.Any(x => x.Id == index_value))
                {
                    VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                    if (item_detail != null && item_detail.Data != null && item_detail.Data.Count > 0)
                    {
                        List<PVDetail> detail_list = item_detail.Data.Cast<PVDetail>().ToList();
                        if (detail_list != null && detail_list.Count > 0)
                        {
                            foreach (PVDetail item in detail_list)
                            {
                                if (item != null && !string.IsNullOrEmpty(item.ma_imei))
                                {
                                    List<string> imei = item.ma_imei.Split(',').ToList();
                                    foreach (string imei_item in imei)
                                    {
                                        if (!string.IsNullOrEmpty(imei_item))
                                            list_imei.Add(new ImeiItem
                                            {
                                                stt_rec0 = item.stt_rec0,
                                                ma_imei = imei_item.Trim(),
                                                ma_vt = item.ma_vt.Trim(),
                                                ma_kho = string.IsNullOrEmpty(item.ma_kho) ? "" : item.ma_kho.Trim(),
                                                gia_nt0 = item.gia_nt,
                                                //ghi_chu = vc_item.dien_giai,
                                                ma_td1 = string.IsNullOrEmpty(item.ma_td1) ? "" : item.ma_td1.Trim(),
                                                budslive = item.budslive,
                                            });
                                    }
                                }
                            }
                        }
                    }
                }

                if (list_imei != null && list_imei.Count > 0)
                {
                    string json = JsonSerializer.Serialize(list_imei);
                    queryIMEI = $"exec Genbyte$IMEI$PostImeiOrder '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.so_ct}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', '{vc_item.ma_ct}', '{json}'";
                    service.ExecuteNonQuery(queryIMEI);
                }
            }          

            model.success = true;
            model.message = "edit_voucher_success";
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
            sql += $"delete from {this.TaxTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from c571_dotran  where stt_rec_pna = @vc_id \n";
            sql += $"delete from c104$000000 where stt_rec = REPLACE(@vc_id, 'PNA', 'DO1') ";
            sql += $"delete from {"m104$" + ngay_ct.ToString("yyyyMM")} where stt_rec = REPLACE(@vc_id, 'PNA', 'DO1') \n";
            sql += $"delete from {"d104$" + ngay_ct.ToString("yyyyMM")} where stt_rec = REPLACE(@vc_id, 'PNA', 'DO1') \n";
            sql += $"delete from {"i104$" + ngay_ct.ToString("yyyyMM")} where stt_rec = REPLACE(@vc_id, 'PNA', 'DO1') \n";
            sql += $"delete from sync_dotran_prime where stt_rec = REPLACE(@vc_id, 'PNA', 'DO1') \n";
            sql += $"delete from sync_dotran_d104 where stt_rec = REPLACE(@vc_id, 'PNA', 'DO1') \n";
            sql += $"delete from imei_order where stt_rec = @vc_id \n";


            paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });

            service.ExecTransactionNonQuery(sql, paras);

            // Thực hiện cập nhật lại trạng thái của phiếu giao hàng DO

            sql = $"update c104$000000 set status = '0' where stt_rec = @vc_id \n";
            sql += $"update {"m104$" + ngay_ct.ToString("yyyyMM")} set status = '0'  where stt_rec = @vc_id \n";
            sql += $"update {"i104$" + ngay_ct.ToString("yyyyMM")} set status = '0'  where stt_rec = @vc_id \n";

            paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("PNA", "DO1")
            });

            service.ExecTransactionNonQuery(sql, paras, ConnectType.Accounting);

            //return
            model.success = true;
            model.message = "delete_voucher_success";
            model.result = voucherId;
            return model;
        }

        #endregion

        #region READ
        /** 
         * Load top bản ghi của chứng từ (không phân trang) 
         */
        #region TopLoading
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
        #endregion

        /** 
         * Lấy dữ liệu của chứng từ theo khóa chính 
         */
        #region GetById
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
	SELECT @q = 'select a.*, b.ten_kh, c.ten_tt, d.ten_gd, e.ten_cuahang from {1}' + @exp + ' a left join dmkh b on a.ma_kh = b.ma_kh left join dmtt c on a.ma_tt = c.ma_tt left join dmmagd d on a.ma_gd = d.ma_gd left join dmcuahang e on a.ma_cuahang = e.ma_cuahang where stt_rec = @stt_rec '
	SELECT @q = @q + CHAR(13) + 'select a.*, b.ten_vt from {2}' + @exp + ' a left join dmvt b on a.ma_vt = b.ma_vt where a.stt_rec = @stt_rec'
    SELECT @q = @q + CHAR(13) + 'select * from {3}' + @exp + ' where stt_rec = @stt_rec'
    SELECT @q = @q + CHAR(13) + 'select * from {4}' + @exp + ' where stt_rec = @stt_rec'
	EXEC sp_executesql @q, N'@stt_rec CHAR(13)', @stt_rec = @stt_rec
END";
            sql = string.Format(sql, this.MasterTable, this.PrimeTable, this.DetailTable, this.TaxTable, this.DiscountTable);
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);

            //convert dataset to model
            if (ds != null && ds.Tables.Count >= 2)
            {
                VoucherItemLoading vc_item = ds.Tables[0].ToList<VoucherItemLoading>().FirstOrDefault();
                IList<PVDetail> pr_detail = ds.Tables[1].ToList<PVDetail>();
                IList<TaxDetail> tax_detail = ds.Tables[2].ToList<TaxDetail>();
                IList<SVDiscountModel> discount_detail = ds.Tables[3].ToList<SVDiscountModel>();

                //ngày hệ thống = ngày hiện tại của server
                vc_item.ngay_ht = DateTime.Today;

                BaseModel invoice_model = new BaseModel();
                invoice_model.masterInfo = vc_item;
                invoice_model.details = new List<DetailItemModel>();
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 1,
                    Name = _DETAIL_PARA,
                    Data = pr_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 2,
                    Name = _TAX_PARA,
                    Data = tax_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 2,
                    Name = _DISCOUNT_PARA,
                    Data = discount_detail
                });

                model.result = invoice_model;
            }

            return model;
        }
        #endregion

        /** 
         * tìm kiếm chứng từ (có xử lý phân trang) 
         */
        #region Finding
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
        #endregion

        /** 
        * Lấy danh sách dữ liệu của danh mục có xử lý phân trang
        * entities: input object có type là EntityCollection<T>
        */
        #region GetByPaging
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
        #endregion

        /** 
       * Lấy dữ liệu khác của từng mã
       * entities: input object có type là EntityCollection<T>
       */
        #region GetOtherData
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

        #region GetImeis
        public List<ImeiState> GetImeis(CommonObjectModel model)
        {
            return new List<ImeiState>();
        }
        #endregion

        #endregion

        #region checkImeiUpdate
        CommonObjectModel checkImeiUpdate(VoucherItem vc_item, BaseModel vc_item_old)
        {
            var listImei = new List<string>();
            var ma_cuahang = "";

            //tách chuỗi imei nhập mới của từng vật tư (string: 001,002,003) để tạo mảng danh sách imei
            if (vc_item.details.Any(x => x.Id == 1))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null && item_detail.Data != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as PVDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            var lst_imei = svDetail.ma_imei.Split(",").ToList();
                            for (int i = 0; i < lst_imei.Count; i++)
                            {
                                lst_imei[i] = lst_imei[i].Trim();
                            }
                            listImei.AddRange(lst_imei);
                            ma_cuahang = svDetail.ma_cuahang;
                        }
                    }

                }
            }

            //tách chuỗi imei đã có ở phiếu cũ (lập chứng từ) của từng vật tư (string: 001,002,003) để tạo mảng danh sách imei
            var listImei_old = new List<string>();
            var ma_cuahang_old = "";
            if (vc_item_old.details.Any(x => x.Id == 1))
            {
                DetailItemModel? item_detail = vc_item_old.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null && item_detail.Data != null)
                {
                    foreach (PVDetail item in (item_detail.Data as List<PVDetail>))
                    {
                        if (item != null && !string.IsNullOrEmpty(item.ma_imei))
                        {
                            var lst_imei = item.ma_imei.Split(",").ToList();
                            for (int i = 0; i < lst_imei.Count; i++)
                            {
                                lst_imei[i] = lst_imei[i].Trim();
                            }
                            listImei_old.AddRange(lst_imei);
                            ma_cuahang_old = item.ma_cuahang;
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

            //lấy trạng thái imei của danh sách imei nhập mới
            Imei.Service imeiService = new Imei.Service();
            List<Imei.ImeiState> state_imei = imeiService.GetStateOfImeis(listImei);

            //danh sách tình trạng tồn tại imei trong hệ thống của các imei
            List<string> exists = state_imei.Where(x => x.exists_yn == true).Select(x => x.ma_imei).ToList();

            //danh sách tình trạng đặt hàng của các imei
            List<string> dat_hang = state_imei.Where(x => x.dat_hang_yn == true).Select(x => x.ma_imei).ToList();
            
            //Nếu trong phiếu nhập mua NCC có imei đã tồn tại trong hệ thống => trả về cảnh báo
            if (exists != null && exists.Count > 0)
            {
                var list_result_error = new List<ResultMessageError>();
                list_result_error.Add(new ResultMessageError
                {
                    name = "%imei",
                    value = string.Join(", ", exists)
                });
                result_model.success = false;
                result_model.message = "exists_yn_yes";
                result_model.result = list_result_error;
            }

            listImei_old.Except(listImei).ToList().ForEach(x =>
            {
                list_imei_delete.Add(new ImeiItem { ma_imei = x });
            });
            dat_hang = dat_hang.Except(listImei_old).ToList();

            //Nếu trong phiếu nhập mua NCC có imei đang ở trạng thái đặt hàng => trả về cảnh báo
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
        #endregion

        #region IsInvalidWarehouse
        /// <summary>
        /// Kiểm tra sự hợp lệ kho với cửa hàng
        /// </summary>
        /// <param name="ma_cuahang"></param>
        /// <param name="ma_kho"></param>
        /// <returns></returns>
        private bool IsInvalidWarehouse(string ma_cuahang, string ma_kho)
        {
            CoreService service = new CoreService();

            // kiểm tra sự tồn tại của ma_kho và ma_cuahang trong bảng dmkho
            string sql = "SELECT 1 FROM dmkho WHERE ma_cuahang = @ma_cuahang AND ma_kho = @ma_kho";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@ma_cuahang",
                    SqlDbType = SqlDbType.Char,
                    Value = ma_cuahang
                },
                new SqlParameter
                {
                    ParameterName = "@ma_kho",
                    SqlDbType = SqlDbType.Char,
                    Value = ma_kho
                }
            };

            // Thực thi truy vấn
            var result = service.ExecSql2List<int>(sql, parameters);

            // Nếu có kết quả (tồn tại), trả về false. Nếu không có kết quả, trả về true.
            return result.Count == 0;
        }
        #endregion
    }
}
