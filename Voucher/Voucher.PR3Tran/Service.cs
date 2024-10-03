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
using System.Text.RegularExpressions;
using Voucher.PR3Tran.Models;
using Genbyte.Component.Voucher.Model;

namespace Voucher.PR3Tran
{
    public class Service : IVoucherService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "PR3";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "c569$000000";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "m569$";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "i569$";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "d569$";
        private const string _DETAIL_PARA = "d569";

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "ma_kh,ma_dvcs,ma_cuahang,ma_ca,ma_cuahang_x;#10$,#20$,#30$, #40$, #50$; , , , , :ma_kho,ma_vt,ma_khon;#10$,#20$,#30$;d569,d569,d569";

        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '000000', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,ngay_ct,rtrim(so_ct) as so_ct,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang,rtrim(ma_cuahang_n) as ma_cuahang_n,rtrim(ma_kho) as ma_kho,rtrim(ma_khon) as ma_khon,rtrim(dien_giai) as dien_giai,t_so_luong, t_tien_nt,t_tien,rtrim(ma_nt) as ma_nt,rtrim(ma_ct) as ma_ct,rtrim(ma_gd) as ma_gd,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(a.ma_cuahang) as ma_cuahang,ngay_ct,rtrim(so_ct) as so_ct,rtrim(a.ma_kh) as ma_kh,rtrim(a.ma_kho) as ma_kho,rtrim(a.ma_khon) as ma_khon,b.ten_kh,rtrim(a.dien_giai) as dien_giai, t_so_luong, t_tien_nt,t_tien,rtrim(ma_nt) as ma_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.ma_gd) as ma_gd,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.comment,z.comment2,'''' as Hash', 'a left join dmkh b on a.ma_kh = b.ma_kh left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct and a.ma_gd = x.loai_gd left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "'''";

        /// <summary>
        /// Khai báo các hành động của user tác động đến service hiện tại: addnew, edit, read, delete
        /// </summary>
        public UserAction Action { get; set; }

        /// <summary>
        /// Khai báo quyền truy cập cho các xử lý CRUD
        /// </summary>
        public AccessRight VoucherRight { get; set; }

        public Service()
        {
            VoucherRight = new AccessRight();
            VoucherRight.AllowRead = true;
            VoucherRight.AllowCreate = true;
            VoucherRight.AllowUpdate = true;
            VoucherRight.AllowDelete = true;

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
            VoucherItem vc_item = Converter.BaseModelToEntity<VoucherItem>(data, this.Action, false);
            if (vc_item == null) return null;
            vc_item.ma_ct = this.VoucherCode;

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

            IF NOT EXISTS(SELECT 1 FROM dmttct WHERE (xedit = 1 OR right_yn = 1) AND ma_ct = @vc_code AND status = @vc_status) BEGIN
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




            if (vc_item.ma_nt == "" || vc_item.ma_nt == null)
            {
                vc_item.ma_nt = "VND";
                vc_item.ty_gia = 1;
            }

            //Cập nhật ngày chứng từ là ngày hiện thời của Server
            vc_item.ngay_ct = DateTime.Today;
            vc_item.ngay_lct = DateTime.Today;
            vc_item.ma_gd = VoucherUtils.isTranferSite(vc_item.ma_kho, vc_item.ma_khon)? "1": "2";
            vc_item.loai_ct = "1";
            vc_item.t_tien = vc_item.t_tien_nt;
            vc_item.t_duyet = 0;
            vc_item.t_huy = 0;
            vc_item.fnote3 = VoucherUtils.isASMApprove(vc_item.ngay_ct, vc_item.ma_cuahang, vc_item.ma_cuahang_n) ? "1": "0";

            vc_item.ma_cuahang_n = Startup.Shop;
            vc_item.ma_ca = Startup.Shift;
            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: PRDetail
            int index_value = 1;
            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<PRDetail>? detail_list = JsonSerializer.Deserialize<List<PRDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        vc_item.t_dong = detail_list.Count;
                        detail_list.ForEach(x =>
                        {
                            x.ngay_ct = vc_item.ngay_ct;
                            x.ma_cuahang = vc_item.ma_cuahang;
                            x.ma_ca = vc_item.ma_ca;
                            x.ma_khon = vc_item.ma_khon;
                            x.ma_kho = vc_item.ma_kho;
                            x.tien = x.tien_nt;
                        });
                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(PRDetail).Name;
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
            string insert_prime_table_query = VoucherUtils.getMaterQuery(new VoucherItem(), prime_table, user_id, 1);
            query += "\n\n";
            //query += $"insert into {prime_table} (stt_rec, ma_ct, so_ct, ngay_ct, ngay_lct, ma_gd, loai_ct, ma_kh, ma_nt, ty_gia, t_so_luong, t_tien2, t_tien_nt2, t_tt, t_tt_nt, ma_thue, t_thue_nt, t_thue, status, ma_dvcs, ma_cuahang, ma_ca, dien_giai, user_id0, user_id2, datetime0, datetime2, ma_nvbh, ma_nvvc) ";
            //query += $" select @stt_rec, @ma_ct, @so_ct, @ngay_ct, @ngay_ct, @ma_gd, @loai_ct, @ma_kh, @ma_nt, @ty_gia, @t_so_luong, @t_tien_nt2, @t_tien_nt2, @t_tt_nt, @t_tt_nt, @ma_thue, @t_thue_nt, @t_thue_nt, @status, @ma_dvcs, @ma_cuahang, @ma_ca, @dien_giai, {user_id}, {user_id}, getdate(), getdate(), @ma_nvbh, @ma_nvvc ";
            query += $"{insert_prime_table_query}";

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
                query += $"insert into {detail_table} (stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho, ma_khon,ma_vi_trin, ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du,tk_vt,so_luong,gia_nt,gia,tien_nt,tien, px_gia_dd, stt_rec_pn, stt_rec0pn, stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9) select stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho, ma_khon,ma_vi_trin, ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du,tk_vt,so_luong,gia_nt,gia,tien_nt,tien, px_gia_dd, stt_rec_pn, stt_rec0pn, stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9 from @{_DETAIL_PARA}";
            }

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

            query = "";

            //update các trường null
            query += $"exec fs_UpdateNullToTable '{prime_table}', '{prime_table}', 'stt_rec = ''{stt_rec}''' \n";
            query += $"exec fs_UpdateNullToTable '{detail_table}', '{detail_table}', 'stt_rec = ''{stt_rec}''' \n";
            service.ExecuteNonQuery(query);

            //insert bảng master (c) & inquiry (i)
            string inquiry_table = this.InquiryTable.Trim() + expression;
            query = $"exec MokaOnline$App$Voucher$UpdateInquiryTable '{this.VoucherCode}', '{inquiry_table}', '{prime_table}', '{detail_table}', 'stt_rec', '{stt_rec}', '{this.Operation}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateGrandTable '{this.VoucherCode}', '{this.MasterTable}', '{prime_table}', 'stt_rec', '{stt_rec}'";
            service.ExecuteNonQuery(query);

            //update lại trường ma_cuahang_n tại bảng c569$ theo ma_cuahang_n tại bảng m569$
            //do cấu trúc các bảng của chứng từ PR3 copy từ các bảng của chứng từ PXB nên là cấu trúc bảng xuất điều chuyển
            //tuy nhiên khi thực hiện nghiệp vụ xin hàng thì user sẽ lập phiếu đề nghị với vai trò là bên nhận hàng (bên nhận gửi đề nghị xin các cửa hàng xuất cho mình)
            

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

            if (vc_item.ma_nt == "" || vc_item.ma_nt == null)
            {
                vc_item.ma_nt = "VND";
                vc_item.ty_gia = 1;
            }

            vc_item.t_tien = vc_item.t_tien_nt;

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: PRDetail
            int index_value = 1;
            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<PRDetail>? detail_list = JsonSerializer.Deserialize<List<PRDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        vc_item.t_dong = detail_list.Count;
                        detail_list.ForEach(x =>
                        {
                            x.ma_cuahang = vc_item.ma_cuahang;
                            x.ma_khon = vc_item.ma_khon;
                            x.ma_kho = vc_item.ma_kho;
                            x.tien = x.tien_nt;
                        });
                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(PRDetail).Name;
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

            IF NOT EXISTS(SELECT 1 FROM dmttct WHERE (xdefault = 1 OR right_yn = 1) AND ma_ct = @vc_code AND status = @vc_status) BEGIN
	             UPDATE @check SET is_success = 0, message = 'status_cannot_update'
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
            VoucherItem? old_voucher = service.ExecSql2List<VoucherItem>(sql, paras).FirstOrDefault();
            if (old_voucher != null)
            {
                //Gán mã ca theo thông tin đăng nhập
                vc_item.ma_ca = Startup.Shift;

                //Gán lại các thông tin từ chứng từ cũ trước khi sửa: ma_dvcs, ma_cuahang, ma_ca, ngay_ct
                //(không cho sửa các trường này)
                vc_item.ma_dvcs = old_voucher.ma_dvcs;
                //vc_item.ma_cuahang = old_voucher.ma_cuahang;
                vc_item.ngay_ct = old_voucher.ngay_ct;
                //bool temp = VoucherUtils.isASMApprove(vc_item.ngay_ct, vc_item.ma_cuahang, vc_item.ma_cuahang_n);
                vc_item.ma_gd = VoucherUtils.isTranferSite(vc_item.ma_kho, vc_item.ma_khon) ? "1" : "2";
                vc_item.fnote3 = VoucherUtils.isASMApprove(vc_item.ngay_ct, vc_item.ma_cuahang, vc_item.ma_cuahang_n) ? "1" : "0";
                foreach (VoucherDetail item in vc_item.details)
                {
                    if (item.Data == null || item.Data.Count == 0)
                        continue;
                    item.Data.ForEach(x =>
                    {
                        x.stt_rec = vc_item.stt_rec;
                        x.ma_ct = old_voucher.ma_ct;
                        x.ma_cuahang = vc_item.ma_cuahang;
                        x.ngay_ct = old_voucher.ngay_ct;
                        x.ma_ca = Startup.Shift;
                    });
                }
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
            //    $" ty_gia = @ty_gia, t_so_luong = @t_so_luong, ma_thue = @ma_thue, t_thue = @t_thue_nt, t_thue_nt = @t_thue_nt, t_tien2 = @t_tien_nt2, t_tien_nt2 = @t_tien_nt2," +
            //    $" t_tt_nt = @t_tt_nt, t_tt = @t_tt_nt, ma_gd = @ma_gd, loai_ct = @loai_ct, user_id2 = {user_id}, datetime2 = getdate(), ma_nvbh = @ma_nvbh, ma_nvvc = @ma_nvvc ";
            //query += $" where stt_rec = @stt_rec";
            query += $"{update_prime_table_query}";

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
                query += $"insert into {detail_table} (stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho, ma_khon,ma_vi_trin, ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du,tk_vt,so_luong,gia_nt,gia,tien_nt,tien, px_gia_dd, stt_rec_pn, stt_rec0pn, stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9) select stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho, ma_khon,ma_vi_trin, ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du,tk_vt,so_luong,gia_nt,gia,tien_nt,tien, px_gia_dd, stt_rec_pn, stt_rec0pn, stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9 from @{_DETAIL_PARA}";
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
	SELECT @q = 'select a.*, b.ten_cuahang, c.ten_cuahang as ten_cuahang_n, d.ten_kho, e.ten_kho as ten_khon FROM {1}' + @exp + ' a left join dmcuahang b on a.ma_cuahang_x = b.ma_cuahang left join dmcuahang c on a.ma_cuahang_n = c.ma_cuahang left join dmkho d on a.ma_kho = d.ma_kho left join dmkho e on a.ma_khon = e.ma_kho where stt_rec = @stt_rec '
	SELECT @q = @q + CHAR(13) + 'select a1.*, a2.ten_vt from {2}' + @exp + ' a1 inner join dmvt a2 on a1.ma_vt = a2.ma_vt where stt_rec = @stt_rec'
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
                IList<PRDetail> pr_detail = ds.Tables[1].ToList<PRDetail>();

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
            string sql = "EXEC Genbyte$SalesVoucher$Finding_PR3 @ngay_bd, @ngay_kt, @ma_cuahang, @ma_ct, @so_ct_bd, @so_ct_kt, @ma_kh, @ma_kho, @ma_kho2, @ma_vt, @ma_imei, @status, @whereClause, @page_index, @page_size, @admin, @user_id, @ext_filter, @order_fields, @filterShopId, @filterShopId_out";
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
                new SqlParameter(){ ParameterName = "@filterShopId_out", SqlDbType = SqlDbType.NVarChar, Value = param.ma_cuahang2?.Trim() },
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
        public CommonObjectModel GetOtherData(string ma_ct, string ma_cuahang)
        {
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
    }
}
