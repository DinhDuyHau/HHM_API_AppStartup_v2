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
//using Voucher.ITTran_PXB2.Model;
using Genbyte.Component.Voucher.Model;
using Microsoft.Extensions.Configuration;
using Genbyte.Base.Security;

namespace Voucher.ITTran_PXB2
{
    public class Service : IVoucherService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "PXB";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "c585$000000";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "m585$";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "i585$";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "d585$";
        private const string _DETAIL_PARA = "d585";

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "ma_kh,ma_dvcs,ma_cuahang,ma_ca,ma_cuahang_n,ma_kho,ma_khon;#10$,#20$,#30$,#40$,#50$,#60$,#65$; , , , , , , :ma_kho,ma_vt,ma_imei,ma_khon;#10$,#20$,#30$,#40$;d585,d585,d585,d585";
        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading_PXB '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '000000', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,ngay_ct,rtrim(so_ct) as so_ct,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang,rtrim(ma_kho) as ma_kho,rtrim(ma_khon) as ma_khon,rtrim(dien_giai) as dien_giai,t_so_luong, t_tien_nt,t_tien,rtrim(ma_nt) as ma_nt,rtrim(ma_ct) as ma_ct,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2,loai_ct', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs, rtrim(a.ma_cuahang) as ma_cuahang, ngay_ct,rtrim(so_ct) as so_ct,rtrim(a.ma_kh) as ma_kh,rtrim(a.ma_kho) as ma_kho,rtrim(a.ma_khon) as ma_khon,b.ten_kh,rtrim(a.dien_giai) as dien_giai, t_so_luong, t_tien_nt,t_tien,rtrim(ma_nt) as ma_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.comment,z.comment2,'''' as Hash', 'a left join dmkh b on a.ma_kh = b.ma_kh left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct and a.loai_ct = x.loai_gd left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "''', N'@@PRIME_EXT_FILTER', '@@SYSID'";

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
        List<ImeiItem> list_imei_delete = new List<ImeiItem>();

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

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: ITDetail
            int index_value = 1;
            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<ITDetail>? detail_list = JsonSerializer.Deserialize<List<ITDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        //cập nhật ngày chứng từ
                        detail_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);

                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(ITDetail).Name;
                }
            }

            result_model = checkImeiInsert(vc_item);
            if (!result_model.success)
            {
                return result_model;
            }

            result_model.result = vc_item;
            */
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
            query += $"insert into {prime_table} (stt_rec, ma_ct, so_ct, ngay_ct, ngay_lct, ma_gd, loai_ct, ma_kh, ma_nt, ty_gia, t_so_luong, t_tien2, t_tien_nt2, t_tt, t_tt_nt, ma_thue, t_thue_nt, t_thue, status, ma_dvcs, ma_cuahang, ma_ca, dien_giai, user_id0, user_id2, datetime0, datetime2) ";
            query += $" select @stt_rec, @ma_ct, @so_ct, @ngay_ct, @ngay_ct, @ma_gd, @loai_ct, @ma_kh, @ma_nt, @ty_gia, @t_so_luong, @t_tien_nt2, @t_tien_nt2, @t_tt_nt, @t_tt_nt, @ma_thue, @t_thue_nt, @t_thue_nt, @status, @ma_dvcs, @ma_cuahang, @ma_ca, @dien_giai, {user_id}, {user_id}, getdate(), getdate() ";

            //insert các bảng chi tiết
            DetailQuery? detail_query = null;
            string detail_table = "";
            if (voucherQuery.Details.Any(x => x.ParaName == _DETAIL_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DETAIL_PARA);
                detail_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_DETAIL_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";
                query += $"insert into {detail_table} (stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, dvt, ma_kho, ma_khon, so_luong, gia_nt, gia_nt2, tien_nt, tien_nt2) select stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, dvt, '@{vc_item.ma_kho}', '@{vc_item.ma_khon}', so_luong, gia_nt2, gia_nt2, tien_nt2, tien_nt2 from @{_DETAIL_PARA}";
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
            */
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
             
            VoucherItem vc_item = Converter.BaseModelToEntity<VoucherItem>(data, this.Action);
            if (vc_item == null) return null;
            vc_item.ma_ct = this.VoucherCode;

            if (vc_item.ma_nt == "" || vc_item.ma_nt == null)
            {
                vc_item.ma_nt = "VND";
                vc_item.ty_gia = 1;
            }

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: ITDetail
            int index_value = 1;
            // Lấy danh sách tất cả các imei
            List<string> imeis = new List<string>();
            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<ITDetail>? detail_list = JsonSerializer.Deserialize<List<ITDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
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
                    item_detail.Detail_Type = typeof(ITDetail).Name;
                }
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

IF NOT EXISTS(SELECT 1 FROM dmttct WHERE (xdefault = 1 OR xedit = 1) AND ma_ct = @vc_code AND status = @status_older) BEGIN
	UPDATE @check SET is_success = 0, message = 'status_changed_cannot_update'
	SELECT * FROM @check
	RETURN
END

IF @vc_status <> @status_older AND EXISTS(SELECT 1 FROM dmttct WHERE xdefault = 1 AND ma_ct = @vc_code AND status = @vc_status) BEGIN
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
            BaseModel res = (BaseModel)this.GetById(vc_item.stt_rec.Replace("'", "''")).result;
            VoucherItem old_voucher = (VoucherItem)res.masterInfo;
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

                foreach (VoucherDetail item in vc_item.details)
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
            result_model = checkImeiUpdate(vc_item, res);
            if (!result_model.success)
            {
                return result_model;
            }
            //return voucher object
            result_model.result = vc_item;
            */
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
            query += $"update {prime_table} set status = @status, ma_ca = @ma_ca, dien_giai = @dien_giai, ma_kh = @ma_kh, ma_nt = @ma_nt," +
                $" ty_gia = @ty_gia, ma_kho = @ma_kho, ma_khon = @ma_khon, t_so_luong = @t_so_luong, t_tien = @t_tien, t_tien_nt = @t_tien_nt," +
                $" ma_gd = @ma_gd, loai_ct = @loai_ct, user_id2 = {user_id}, datetime2 = getdate()";
            query += $" where stt_rec = @stt_rec";

            //xóa và insert lại các bảng chi tiết
            DetailQuery? detail_query = null;
            string detail_table = "";
            if (voucherQuery.Details.Any(x => x.ParaName == _DETAIL_PARA))
            {
                detail_query = voucherQuery.Details.FirstOrDefault(x => x.ParaName == _DETAIL_PARA);
                detail_table = detail_query?.TableName + (detail_query.Partition_yn ? expression : "");

                query += "\n\n";
                query += detail_query?.QueryString;
                query += "\n";
                query += $"update @{_DETAIL_PARA} set line_nbr = row_id$, stt_rec0 = right(row_id$ + 1000, 3), stt_rec = @stt_rec, ma_ct = @ma_ct, ngay_ct = @ngay_ct, so_ct = @so_ct, ma_cuahang = @ma_cuahang, ma_ca = @ma_ca where 1=1";
                query += "\n\n";

                //xóa dữ liệu cũ (bảng detail) và insert dữ liệu mới
                query += $"delete from {detail_table} where stt_rec = @stt_rec \n";
                query += $"insert into {detail_table} (stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, dvt, ma_imei, ma_kho, ma_khon, so_luong, gia, tien, gia_nt, tien_nt, tk_vt, tk_du, ma_nx, px_gia_dd, stt_rec_yc, stt_rec0yc) ";
                query += $"select stt_rec, stt_rec0, ma_cuahang, ma_ca, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, dvt, ma_imei, ma_kho, ma_khon, so_luong, gia_nt, tien_nt, gia_nt, tien_nt, tk_vt, tk_du, ma_nx, px_gia_dd, stt_rec_yc, stt_rec0yc from @{_DETAIL_PARA}";
            }
            query += "\n\n";
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


            // Lấy danh sách IMEI
            List<ImeiItem> list_imei = new List<ImeiItem>();
            string stt_rec_yc = "";
            // id = 1 ==> type: PVDetail
            int index_value = 1;
            if (vc_item.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_detail != null)
                {
                    List<ITDetail> detail_list = item_detail.Data.Cast<ITDetail>().ToList();
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        foreach (var item in detail_list)
                        {
                            stt_rec_yc = item.stt_rec_yc;
                            if (!string.IsNullOrEmpty(item.ma_imei))
                            {
                                List<string> imei = item.ma_imei.Split(',').ToList();
                                foreach (var imei_item in imei)
                                {
                                    list_imei.Add(new ImeiItem
                                    {
                                        ma_imei = imei_item.Trim(),
                                        ma_vt = item.ma_vt,
                                        ma_kho = vc_item.ma_kho,
                                        gia_nt0 = item.gia_nt,
                                        ghi_chu = vc_item.dien_giai
                                    });
                                }
                            }
                        }
                    }
                }
            }
            //Update dat_hang_yn = 1 đồng thời cập nhật lại các imei đã đặt hàng trước đó nhưng lại dùng imei khác
            string queryIMEI = "";
            if (list_imei_delete.Count > 0)
            {
                string json_del = JsonSerializer.Serialize(list_imei_delete);
                queryIMEI = $"exec Genbyte$IMEI$UpdateState$Inventory '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', 0, '{json_del}'";
                service.ExecuteNonQuery(queryIMEI);
            }

            list_imei.ForEach(item =>
            {
                item.ma_vt = item.ma_vt.Trim();
                item.ma_kho = item.ma_kho.Trim();
            });

            if (vc_item.status == "0")
            {
                string json = JsonSerializer.Serialize(list_imei);
                //create query insert IMEI
                queryIMEI = $"exec Genbyte$IMEI$UpdateState$Inventory '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', 1, '{json}'";
                service.ExecuteNonQuery(queryIMEI);
            }
            //Nếu trạng thái là hoàn thành thì đẩy vào imei vào hệ thống
            else if (vc_item.status == "2")
            {
                string json = JsonSerializer.Serialize(list_imei);
                //create query insert IMEI
                queryIMEI = $"exec Genbyte$IMEI$PXB$Update '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', '{json}'";
                service.ExecuteNonQuery(queryIMEI);

                queryIMEI = $"EXEC MokaOnline$Voucher$PXBUpdatePNF '{stt_rec}'";
                service.ExecuteNonQuery(queryIMEI);
            }
            if(vc_item.status != "0")
            {
                updateStatusTranfer(stt_rec, stt_rec_yc, vc_item.status);
            }
            model.success = true;
            model.message = "edit_voucher_success";
            model.result = vc_item;
            */
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

            //check exists & trạng thái chứng từ = 0 (PXB & PNF)
            string sql = @$"declare @vc_id char(13), @stt_rec_pnf char(13), @state bit, @err_message nvarchar(500), @ngay_ct datetime
set @vc_id = '{voucherId.Replace("'", "''")}'
set @stt_rec_pnf = replace(@vc_id, '{this.VoucherCode}', 'PNF')
select @state = 1, @err_message = ''
if exists(select 1 from {this.MasterTable} where status = '0' and stt_rec = @vc_id) begin
    select @ngay_ct = ngay_ct from {this.MasterTable} where stt_rec = @vc_id
end
else begin
    select @state = 0, @err_message = 'err_pxb_not_exists_or_status_changed'
    select @state as [state], @err_message as err_message, @ngay_ct as ngay_ct
    return
end
if exists(select 1 from c575$000000 where status <> '0' and stt_rec = @stt_rec_pnf) begin
    select @state = 0, @err_message = 'err_delete_pnf_status_changed'
    select @state as [state], @err_message as err_message, @ngay_ct as ngay_ct
    return
end
select @state as [state], @err_message as err_message, @ngay_ct as ngay_ct
";
            DataSet ds = service.ExecSql2DataSet(sql);
            if (ds != null || ds.Tables.Count > 0 || ds.Tables[0].Rows.Count > 0)
            {
                bool state = Convert.ToBoolean(ds.Tables[0].Rows[0]["state"]);
                string err_message = ds.Tables[0].Rows[0]["err_message"].ToString();
                if(!state)
                {
                    model.success = false;
                    model.message = err_message;
                    model.result = null;
                    return model;
                }
            }

            //Thực hiện xóa có sử dụng transaction
            DateTime ngay_ct = Convert.ToDateTime(ds.Tables[0].Rows[0]["ngay_ct"]);
            sql = $"exec fs_Voucher$RemoveInv$Imei '{voucherId.Replace("'", "''")}', '{this.VoucherCode}' \n";
            sql += $"delete from {this.MasterTable} where stt_rec = @vc_id \n";
            sql += $"delete from {this.PrimeTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.InquiryTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"delete from {this.DetailTable + ngay_ct.ToString("yyyyMM")} where stt_rec = @vc_id \n";
            sql += $"EXEC MokaOnline$Voucher$PXBDeletePNF @vc_id\n";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });
            service.ExecTransactionNonQuery(sql, paras);

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
        public CommonObjectModel TopLoading(List<Dictionary<string, object>> data)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };

            //convert
            LoadingParam param = Converter.DictionaryToGenericEntityParam<LoadingParam>(data);

            CoreService core_service = new CoreService();

            //get sys database name
            string sys_dbname = "";
            using (SqlConnection sys_conn = core_service.CreateDbConn(ConnectType.Sys))
            {
                sys_dbname = sys_conn.Database;
            }

            //lọc trường fnote2 tại bảng m585 với giá trị 1 hoặc 2 (1: luân chuyển kho tại cửa hàng, 2: điều chuyển hàng lỗi về cty)
            string filter_fnote2 = "1,2";

            string sql = this.LoadingQuery.Replace("@@VOUCHER_CODE", this.VoucherCode);
            sql = sql.Replace("@@MASTER_TABLE", this.MasterTable);
            sql = sql.Replace("@@PRIME_TABLE", this.PrimeTable);
            sql = sql.Replace("@@ORDER_BY", param.order_by);
            sql = sql.Replace("@@SYSDATABASE", sys_dbname);
            sql = sql.Replace("@@ADMIN", Startup.Admin.ToString());
            sql = sql.Replace("@@USER_ID", Startup.UserId.ToString());
            sql = sql.Replace("@@SHOP_ID", Startup.Shop.ToString());
            sql = sql.Replace("@@PRIME_EXT_FILTER", filter_fnote2);
            sql = sql.Replace("@@SYSID", "ITTran_PXB2");

            DataSet dataset = core_service.ExecSql2DataSet(sql);

            // Chuyển đổi dataset thành List<Dictionary<string, object>>
            List<Dictionary<string, object>> dataByTable = new List<Dictionary<string, object>>();

            // Tên bảng response
            string[] tableNames = { "voucher", "payment_method", "authorization" };

            for (int i = 0; i < dataset.Tables.Count; i++)
            {
                DataTable table = dataset.Tables[i];
                List<Dictionary<string, object>> tableData = this.ConvertDataTableToList(table);

                // Mã hóa trường stt_rec trong từng bảng
                tableData.ForEach(x =>
                {
                    // check nếu có stt_rec mới thực hiện mã hóa
                    if (x.ContainsKey("stt_rec") && x["stt_rec"] is string sttRecValue)
                    {
                        x["stt_rec"] = APIService.EncryptForWebApp(
                            (string)x["stt_rec"],
                            _configuration["Security:KeyAES"],
                            _configuration["Security:IVAES"]
                        );
                    }
                });

                string tableName = (i < tableNames.Length) ? tableNames[i] : $"data_{i}";
                dataByTable.Add(new Dictionary<string, object>
                {
                    { tableName, tableData }
                });
            }

            if (dataByTable != null && dataByTable.Count > 0)
            {
                model.success = true;
                model.result = dataByTable;
            }
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
            /*
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(voucherId))
                throw new Exception(ApiReponseMessage.Error_InputData);

            //Lấy dữ liệu từ bảng prime và detail theo id truyền vào
            string sql = @"DECLARE @q NVARCHAR(4000), @stt_rec CHAR(13), @exp CHAR(6)
SET @stt_rec = @vc_id
IF EXISTS(SELECT 1 FROM {0} WHERE stt_rec = @stt_rec) BEGIN
	SELECT @exp = CONVERT(CHAR(6), ngay_ct, 112) FROM {0} WHERE stt_rec = @stt_rec
	SELECT @q = 'select a.*, b.ten_cuahang, c.ten_cuahang as ten_cuahang_n, d.ten_kho, e.ten_kho as ten_khon from {1}' + @exp + ' a left join dmcuahang b on a.ma_cuahang = b.ma_cuahang left join dmcuahang c on a.ma_cuahang_n = c.ma_cuahang left join dmkho d on a.ma_kho = d.ma_kho left join dmkho e on a.ma_khon = e.ma_kho where stt_rec = @stt_rec '
	SELECT @q = @q + CHAR(13) + 'select a.*, b.ten_vt from {2}' + @exp + ' a left join dmvt b on a.ma_vt = b.ma_vt where a.stt_rec = @stt_rec'
	EXEC sp_executesql @q, N'@stt_rec CHAR(13)', @stt_rec = @stt_rec
END";
            sql = string.Format(sql, this.MasterTable, this.PrimeTable, this.DetailTable);
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
                IList<ITDetail> pr_detail = ds.Tables[1].ToList<ITDetail>();

                BaseModel invoice_model = new BaseModel();
                invoice_model.masterInfo = vc_item;
                invoice_model.details = new List<DetailItemModel>();
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 1,
                    Name = _DETAIL_PARA,
                    Data = pr_detail
                });

                model.result = invoice_model;
            }
            */

            return model;
        }

        /** 
         * tìm kiếm chứng từ (có xử lý phân trang) 
         */
        public CommonObjectModel Finding(List<Dictionary<string, object>> data)
        {
            //convert
            FindExtParam param = Converter.DictionaryToFindExtParam(data);

            EntityCollection<Dictionary<string, object>> entities = new EntityCollection<Dictionary<string, object>>()
            {
                PageCount = 0,
                PageIndex = param.page_index,
                PageSize = param.page_size,
                RecordCount = 0,
                Items = new List<Dictionary<string, object>>()
            };

            CoreService core_service = new CoreService();
            string sql = "EXEC Genbyte$SalesVoucher$Finding_PXB @ngay_bd, @ngay_kt, @ma_cuahang, @ma_ct, @so_ct_bd, @so_ct_kt, @ma_kh, @ma_kho, @ma_kho2, @ma_vt, @ma_imei, @status, @whereClause, @page_index, @page_size, @admin, @user_id, @ext_filter, @order_fields, @filterShopId, @filterShopId_in, @prime_ext_filter, @status2, @SysID";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
                new SqlParameter(){ ParameterName = "@ngay_bd", SqlDbType = SqlDbType.DateTime, Value = param.ngay_bd },
                new SqlParameter(){ ParameterName = "@ngay_kt", SqlDbType = SqlDbType.DateTime, Value = param.ngay_kt },
                new SqlParameter(){ ParameterName = "@ma_cuahang", SqlDbType = SqlDbType.VarChar, Value = Startup.Shop },
                new SqlParameter(){ ParameterName = "@ma_ct", SqlDbType = SqlDbType.VarChar, Value = param.ma_ct?.Trim() },
                new SqlParameter(){ ParameterName = "@so_ct_bd", SqlDbType = SqlDbType.VarChar, Value = param.so_ct_bd },
                new SqlParameter(){ ParameterName = "@so_ct_kt", SqlDbType = SqlDbType.VarChar, Value = param.so_ct_kt },
                new SqlParameter(){ ParameterName = "@ma_kh", SqlDbType = SqlDbType.VarChar, Value = param.ma_kh?.Trim() },
                new SqlParameter(){ ParameterName = "@ma_kho", SqlDbType = SqlDbType.VarChar, Value = param.ma_kho?.Trim() },
                new SqlParameter(){ ParameterName = "@ma_kho2", SqlDbType = SqlDbType.VarChar, Value = param.ma_kho2?.Trim() },
                new SqlParameter(){ ParameterName = "@ma_vt", SqlDbType = SqlDbType.VarChar, Value = param.ma_vt?.Trim() },
                new SqlParameter(){ ParameterName = "@ma_imei", SqlDbType = SqlDbType.VarChar, Value = param.ma_imei?.Trim() },
                new SqlParameter(){ ParameterName = "@status", SqlDbType = SqlDbType.VarChar, Value = param.status },
                new SqlParameter(){ ParameterName = "@whereClause", SqlDbType = SqlDbType.NVarChar, Value = param.where_clause },
                new SqlParameter(){ ParameterName = "@page_index", SqlDbType = SqlDbType.Int, Value = param.page_index },
                new SqlParameter(){ ParameterName = "@page_size", SqlDbType = SqlDbType.Int, Value = param.page_size },
                new SqlParameter(){ ParameterName = "@admin", SqlDbType = SqlDbType.Bit, Value = Startup.Admin == 0 ? false : true },
                new SqlParameter(){ ParameterName = "@user_id", SqlDbType = SqlDbType.Int, Value = Startup.UserId },
                new SqlParameter(){ ParameterName = "@ext_filter", SqlDbType = SqlDbType.NVarChar, Value = param.ext_filter },
                new SqlParameter(){ ParameterName = "@order_fields", SqlDbType = SqlDbType.NVarChar, Value = "status, so_ct desc, ngay_ct desc, stt_rec desc" },
                new SqlParameter(){ ParameterName = "@filterShopId", SqlDbType = SqlDbType.NVarChar, Value = param.ma_cuahang?.Trim() },
                new SqlParameter(){ ParameterName = "@filterShopId_in", SqlDbType = SqlDbType.NVarChar, Value = param.ma_cuahang2?.Trim() },
                //lọc trường fnote2 tại bảng m585 với giá trị 1 hoặc 2
                //(1: luân chuyển kho tại cửa hàng, 2: điều chuyển hàng lỗi về cty)
                new SqlParameter(){ ParameterName = "@prime_ext_filter", SqlDbType = SqlDbType.NVarChar, Value = "1,2" },
                new SqlParameter(){ ParameterName = "@status2", SqlDbType = SqlDbType.VarChar, Value = param.status2 },
                new SqlParameter(){ ParameterName = "@SysID", SqlDbType = SqlDbType.NVarChar, Value = param.sysid?.Trim() },
            });
            DataSet dataSet = core_service.ExecSql2DataSet(sql, paras);
            if (dataSet != null && dataSet.Tables.Count > 1)
            {
                entities.PageCount = (int)dataSet.Tables[0].Rows[0]["TotalPage"];
                entities.RecordCount = (int)dataSet.Tables[0].Rows[0]["TotalRecordCount"];
                entities.PageSize = (int)dataSet.Tables[0].Rows[0]["PageSize"];
                entities.Items = Converter.TableToListDictionary(dataSet.Tables[1]);
            }

            CommonObjectModel model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = entities
            };

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
        public List<ImeiState> GetImeis(CommonObjectModel model)
        {
            return new List<ImeiState>();
        }
        #endregion

        CommonObjectModel checkImeiInsert(VoucherItem vc_item)
        {
            /*
            var listImei = new List<string>();
            var ma_cuahang = "";
            if (vc_item.details.Any(x => x.Id == 1))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as ITDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            listImei.Add(svDetail.ma_imei.Trim());
                            ma_cuahang = svDetail.ma_cuahang;
                        }
                    }

                }
            }
            */
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
            /*
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
            */
            return result_model;
        }
        CommonObjectModel checkImeiUpdate(VoucherItem vc_item, BaseModel vc_item_old)
        {
            CommonObjectModel result_model = new CommonObjectModel()
            {
                success = true,
                message = "",
                result = null
            };
/*
            if (vc_item.status != "2" && vc_item.status != "0")
            {
                return result_model;
            }
            var listImei = new List<string>();
            var ma_cuahang = "";
            if (vc_item.details.Any(x => x.Id == 1))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {
                    foreach (var item in item_detail.Data)
                    {
                        var svDetail = item as ITDetail;
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

            var listImei_old = new List<string>();
            var ma_cuahang_old = "";
            if (vc_item_old.details.Any(x => x.Id == 1))
            {
                DetailItemModel? item_detail = vc_item_old.details.FirstOrDefault(x => x.Id == 1);

                if (item_detail != null)
                {

                    foreach (var item in item_detail.Data as List<ITDetail>)
                    {
                        var svDetail = item as ITDetail;
                        if (svDetail != null && !string.IsNullOrEmpty(svDetail.ma_imei))
                        {
                            var lst_imei = svDetail.ma_imei.Split(",").ToList();
                            for (int i = 0; i < lst_imei.Count; i++)
                            {
                                lst_imei[i] = lst_imei[i].Trim();
                            }
                            listImei_old.AddRange(lst_imei);
                            ma_cuahang_old = svDetail.ma_cuahang;
                        }
                    }

                }
            }


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
                list_imei_delete.Add(new ImeiItem { ma_imei = x });
            });

            dat_hang = dat_hang.Except(listImei_old).ToList();
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
*/
            return result_model;
        }
        //Cập nhật lịch sử giao dịch và update trạng thái của phiếu đề nghị
        public void updateStatusTranfer(string stt_rec, string stt_rec_yc, string status)
        {
/*
            CoreService service = new CoreService();
            string sql = @"declare @exp CHAR(6), @q nvarchar(4000)
                           declare @count int, @t_duyet int, @t_huy int, @t_hoanthanh int, @status_xh char(1)
                           select @exp = CONVERT(CHAR(6), ngay_ct, 112) from c106$000000 where stt_rec = @stt_rec_yc
                           select * into #temp from log_lsgdxhdc where stt_rec_dc = @stt_rec and status_dc = '0'
                           update #temp set crdate_dc = getdate(), status_dc = @status, status_xh = @status
                           insert into log_lsgdxhdc select * from #temp

                           if @status = '5' begin
                                SET @q = 'update d106$' +@exp+ ' set tag1 = 0, tag2 = 0, ngay_hh_dc = DATEADD(day, 1, GETDATE()), xstatus = '''+@status+''' where stt_rec = '''+@stt_rec_yc+''' and stt_rec_dc = '''+@stt_rec+''' and xstatus = ''2'' '
                                EXEC sp_executesql @q
                            end
                            else if @status = '4' begin
                                SET @q = 'update d106$' +@exp+ ' set ngay_huy = GETDATE(), xstatus = '''+@status+''' where stt_rec = '''+@stt_rec_yc+''' and stt_rec_dc = '''+@stt_rec+''' and xstatus = ''2'' '
                                EXEC sp_executesql @q
                            end
                            else if @status = '2' begin
                                SET @q = 'update d106$' +@exp+ ' set ngay_xuat = GETDATE(), xstatus = ''3'' where stt_rec = '''+@stt_rec_yc+''' and stt_rec_dc = '''+@stt_rec+''' and xstatus = ''2'' '
                                EXEC sp_executesql @q
                            end
                            
                             select top 0 * into #d106 from d106$000000 
                             SET @q = 'insert into #d106 select * from d106$' +@exp+ ' where stt_rec = '''+@stt_rec_yc+''' '
                                EXEC sp_executesql @q
                            
                            -- Cập nhật lại trạng thái cho chứng từ
                            SELECT @count = COUNT(1) FROM #d106
			                SELECT @t_duyet = COUNT(1) FROM #d106 WHERE tag1 = 1
			                SELECT @t_hoanthanh = COUNT(1) FROM #d106 WHERE xstatus = '3' or xstatus = '4' or xstatus = '1'
			                SELECT @t_huy = COUNT(1) FROM #d106 WHERE tag2 = 1
			                IF @t_huy = @count BEGIN 
				                SELECT @status_xh = '3'
			                END
			                ELSE IF @t_hoanthanh = @count and @t_duyet + @t_huy = @count BEGIN 
				                SELECT @status_xh = '2'
			                END
			                ELSE BEGIN 
				                SELECT @status_xh = '0'
			                END
                            
                             SET @q = 'update m106$' +@exp+ ' set status = '''+@status_xh+''', t_duyet = '+CAST(@t_duyet AS VARCHAR)+', t_huy = '+CAST(@t_huy AS VARCHAR)+'  where stt_rec = '''+@stt_rec_yc+''' '
                             SET @q = @q + CHAR(13) + ' update c106$000000 set status = '''+@status_xh+''' where stt_rec = '''+@stt_rec_yc+''' '
                             SET @q = @q + CHAR(13) + ' update i106$' +@exp+ ' set status = '''+@status_xh+''' where stt_rec = '''+@stt_rec_yc+''' '
                             
                             EXEC sp_executesql @q

                            drop table #d106
                            drop table #temp
                          ";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@stt_rec",
                SqlDbType = SqlDbType.Char,
                Value = stt_rec
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@stt_rec_yc",
                SqlDbType = SqlDbType.Char,
                Value = stt_rec_yc
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@status",
                SqlDbType = SqlDbType.Char,
                Value = status
            });
            service.ExecTransactionNonQuery(sql, paras, ConnectType.Accounting);
*/
        }

        /** Hàm chuyển đổi DataTable thành List<Dictionary<string, object>> */
        #region ConvertDataTableToList
        public List<Dictionary<string, object>> ConvertDataTableToList(DataTable table)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            return list;
        }
        #endregion

    }
}
