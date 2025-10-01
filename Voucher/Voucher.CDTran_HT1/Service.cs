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
using Voucher.CDTran_HT1.Models;
using Microsoft.Extensions.Configuration;

namespace Voucher.CDTran_HT1
{
    public class Service : IVoucherService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "HT1";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "phdnhoantien";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "phdnhoantien";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "phdnhoantien";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "phdnhoantien";
        private const string _DETAIL_PARA = "phdnhoantien";

        public string ma_ct_goc { get; } = "BN1";

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "";

        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(ma_ca) as ma_ca,ngay_ct,ngay_duyet,rtrim(so_ct) as so_ct,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang,rtrim(Dien_giai) as dien_giai,tien_hoan,tien_hoan_nt,rtrim(ma_ct) as ma_ct,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(a.ma_ca) as ma_ca,c.ten_ca, rtrim(a.ma_cuahang) as ma_cuahang, ngay_ct,ngay_duyet,rtrim(so_ct) as so_ct,rtrim(a.ma_kh) as ma_kh,b.ten_kh,rtrim(a.Dien_giai) as dien_giai,tien_hoan,tien_hoan_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.comment,z.comment2,'''' as Hash', 'a left join vdmkh_acc b on a.ma_kh = b.ma_kh left join dmca c on a.ma_ca = c.ma_ca left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "''', '@@SYSID'";

        /// <summary>
        /// Khai báo các hành động của user tác động đến service hiện tại: addnew, edit, read, delete
        /// </summary>
        public UserAction Action { get; set; }

        /// <summary>
        /// Khai báo quyền truy cập cho các xử lý CRUD
        /// </summary>
        public AccessRight VoucherRight { get; set; }

        private readonly IConfiguration _configuration;

        public Service(IConfiguration configuration)
        {
            Authoriztion authoriztion = CommonService.GetAuthoriztion("CDTran_HT1");
            VoucherRight = new AccessRight();
            VoucherRight.AllowReadAll = authoriztion.view_yn;
            VoucherRight.AllowRead = authoriztion.access_yn;
            VoucherRight.AllowCreate = authoriztion.add_yn;
            VoucherRight.AllowUpdate = authoriztion.edit_yn;
            VoucherRight.AllowDelete = authoriztion.del_yn;
            this._configuration = configuration;
        }

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
            
            //default
            vc_item.ma_ct = this.VoucherCode;
            vc_item.ma_cuahang = Startup.Shop;
            vc_item.ma_ca = Startup.Shift;
            vc_item.ma_nvbh = Startup.UserName;

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

            // Lấy thông tin thanh toán theo mã FT gửi lên trong request,
            // đối chiếu thông tin lấy được từ mã FT với request data: mã khách hàng, tiền còn được phép hoàn
            sql = "exec Genbyte$Payment$CheckRefundRequest @ma_ft, @ma_kh, @ma_cuahang, @tien_hoan";
            paras = new List<SqlParameter>();
            #region add parameters
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ft",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.ma_ft.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.ma_kh.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.ma_cuahang.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@tien_hoan",
                SqlDbType = SqlDbType.Decimal,
                Value = vc_item.tien_hoan
            });
            #endregion
            check_result = service.ExecSql2List<CheckResult>(sql, paras).FirstOrDefault()!;
            if (!check_result.is_success)
            {
                result_model.success = false;
                result_model.message = check_result.message;
                return result_model;
            }

            //Cập nhật ngày chứng từ là ngày hiện thời của Server
            vc_item.ngay_ct = DateTime.Today;
            vc_item.ngay_lct = DateTime.Today;
            

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
            string prime_table = this.PrimeTable.Trim();
            string insert_prime_table_query = VoucherUtils.getMaterQuery(new VoucherItem(), prime_table, user_id, 1);
            query += "\n\n";
            query += $"{insert_prime_table_query}";

            // Nếu lưu phiếu ở trạng thái 'Chờ duyệt' => cập nhật lại diễn giải theo format và bỏ dấu tiếng việt
            query += "\n\n";
            query += $@"if @status <> '0' begin
    update {prime_table} set dien_giai = dbo.MokaOnline$Function$Message$ConvertToUnsign(dien_giai) where stt_rec = @stt_rec
    update {prime_table} set dien_giai = rtrim(@ma_cuahang) + '_' + rtrim(@ma_kh) + left(dien_giai, 26)
        where stt_rec = @stt_rec
end";
            // Cập nhật các thông tin từ bảng thanh toán QR
            query += "\n\n";
            query += $@"select top 1 * into #ttqr from ct570_ttqr where ma_ft = @ma_ft
if exists(select 1 from #ttqr) begin
	update {prime_table} set stt_rec_tt = b.stt_rec, stt_rec0tt = b.stt_rec0, 
            ma_ct_tt = b.ma_ct, so_ct_tt = b.so_ct, ngay_ct_tt = b.ngay_ct,
			ma_thanhtoan = b.ma_thanhtoan, tien = b.tien, tien_nt = b.tien,
			ref_code = b.ref_code, terminal_id = b.terminal_label
		from {prime_table} a inner join #ttqr b on a.ma_ft = b.ma_ft
end";

            query += "\n\n";
            query += "select @stt_rec as stt_rec, @ma_ct as ma_ct";

            //thực thi query insert vào bảng prime và detail có sử dụng transaction
            CoreService service = new CoreService();
            DataSet ds = service.ExecTransactionSql2DataSet(query);
            string stt_rec = ds.Tables[0].Rows[0]["stt_rec"].ToString();
            string ma_ct = ds.Tables[0].Rows[0]["ma_ct"].ToString();

            //update stt_rec cho đối tượng đang thực hiện thêm mới
            vc_item.stt_rec = stt_rec;

            //update các trường null
            query = $"exec fs_UpdateNullToTable '{prime_table}', '{prime_table}', 'stt_rec = ''{stt_rec}''' \n";
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

            //default
            vc_item.ma_ct = this.VoucherCode;
            vc_item.ma_cuahang = Startup.Shop;
            vc_item.ma_ca = Startup.Shift;
            vc_item.ma_nvbh = Startup.UserName;

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

            // Lấy thông tin thanh toán theo mã FT gửi lên trong request,
            // đối chiếu thông tin lấy được từ mã FT với request data: mã khách hàng, tiền còn được phép hoàn
            sql = "exec Genbyte$Payment$CheckRefundRequest @ma_ft, @ma_kh, @ma_cuahang, @tien_hoan, @exclude_vcID";
            paras = new List<SqlParameter>();
            #region add parameters
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ft",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.ma_ft.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.ma_kh.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.ma_cuahang.Replace("'", "''")
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@tien_hoan",
                SqlDbType = SqlDbType.Decimal,
                Value = vc_item.tien_hoan
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@exclude_vcID",
                SqlDbType = SqlDbType.Char,
                Value = vc_item.stt_rec.Replace("'", "''")
            });
            #endregion
            check_result = service.ExecSql2List<CheckResult>(sql, paras).FirstOrDefault()!;
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
                vc_item.ma_cuahang = old_voucher.ma_cuahang;
                vc_item.ngay_ct = old_voucher.ngay_ct;
                vc_item.ma_nvbh = old_voucher.ma_nvbh;
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
            string prime_table = this.PrimeTable.Trim();
            string update_prime_table_query = VoucherUtils.getMaterQuery(new VoucherItem(), prime_table, user_id, 2);
            query += "\n\n";
            query += $"{update_prime_table_query}";

            // Nếu lưu phiếu ở trạng thái 'Chờ duyệt' => cập nhật lại diễn giải theo format và bỏ dấu tiếng việt
            query += "\n\n";
            query += $@"if @status <> '0' begin
    update {prime_table} set dien_giai = dbo.MokaOnline$Function$Message$ConvertToUnsign(dien_giai) where stt_rec = @stt_rec
    update {prime_table} set dien_giai = rtrim(@ma_cuahang) + '_' + rtrim(@ma_kh) + left(dien_giai, 26) 
        where stt_rec = @stt_rec
end";
            // Cập nhật các thông tin từ bảng thanh toán QR
            query += "\n\n";
            query += $@"select top 1 * into #ttqr from ct570_ttqr where ma_ft = @ma_ft
if exists(select 1 from #ttqr) begin
	update {prime_table} set stt_rec_tt = b.stt_rec, stt_rec0tt = b.stt_rec0, 
            ma_ct_tt = b.ma_ct, so_ct_tt = b.so_ct, ngay_ct_tt = b.ngay_ct,
			ma_thanhtoan = b.ma_thanhtoan, tien = b.tien, tien_nt = b.tien,
			ref_code = b.ref_code, terminal_id = b.terminal_label
		from {prime_table} a inner join #ttqr b on a.ma_ft = b.ma_ft
end";

            query += "\n\n";
            query += "select @stt_rec as stt_rec";

            //thực thi query update bảng prime và insert lại bảng detail có sử dụng transaction
            CoreService service = new CoreService();
            DataSet ds = service.ExecTransactionSql2DataSet(query);
            string stt_rec = ds.Tables[0].Rows[0]["stt_rec"].ToString();

            //update stt_rec cho đối tượng đang thực hiện
            vc_item.stt_rec = stt_rec;

            //update các trường null
            query = $"exec fs_UpdateNullToTable '{prime_table}', '{prime_table}', 'stt_rec = ''{stt_rec}''' \n";
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
            sql = $"delete from {this.MasterTable} where stt_rec = @vc_id \n";
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
            string sql = "exec Genbyte$Payment$GetRefundById @vc_id";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_id",
                SqlDbType = SqlDbType.Char,
                Value = voucherId.Replace("'", "''")
            });
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);

            //convert dataset to model
            if (ds != null && ds.Tables.Count >= 1)
            {
                VoucherItemLoading vc_item = ds.Tables[0].ToList<VoucherItemLoading>().FirstOrDefault();

                BaseModel invoice_model = new BaseModel();
                invoice_model.masterInfo = vc_item;
                invoice_model.details = new List<DetailItemModel>();
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 1,
                    Name = _DETAIL_PARA,
                    Data = null
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