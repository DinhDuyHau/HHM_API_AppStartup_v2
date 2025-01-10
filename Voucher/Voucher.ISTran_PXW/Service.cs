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
using Voucher.ISTran_PXW.Models;
using Genbyte.Component.Voucher.Model;
using Microsoft.Extensions.Configuration;

namespace Voucher.ISTran_PXW
{
    public class Service : IVoucherService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "PXW";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "c563$000000";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "m563$";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "i563$";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "d563$";
        private const string _DETAIL_PARA = "d563";

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "ma_kh,ma_dvcs,ma_cuahang,ma_ca;#10$,#20$,#30$, #40$; , , , :ma_kho,ma_vt,ma_imei;#10$,#20$,#30$;d563,d563,d563";

        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '000000', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(ma_ca) as ma_ca,ngay_ct,rtrim(so_ct) as so_ct,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang,rtrim(dien_giai) as dien_giai, t_so_luong,t_tien_nt,rtrim(ma_nt) as ma_nt,rtrim(ma_ct) as ma_ct,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(a.ma_ca) as ma_ca,c.ten_ca, rtrim(a.ma_cuahang) as ma_cuahang, ngay_ct,rtrim(so_ct) as so_ct,rtrim(a.ma_kh) as ma_kh,b.ten_kh,rtrim(a.dien_giai) as dien_giai, t_so_luong,t_tien_nt,rtrim(ma_nt) as ma_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.comment,z.comment2,'''' as Hash', 'a left join dmkh b on a.ma_kh = b.ma_kh left join dmca c on a.ma_ca = c.ma_ca left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id ', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "''', '@@SYSID'";

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
             */
            VoucherItem vc_item = Converter.BaseModelToEntity<VoucherItem>(data, this.Action);
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




            if (vc_item.ma_nt == "" || vc_item.ma_nt == null)
            {
                vc_item.ma_nt = "VND";
                vc_item.ty_gia = 1;
            }

            //Cập nhật ngày chứng từ là ngày hiện thời của Server
            vc_item.ngay_ct = DateTime.Today;
            vc_item.ngay_lct = DateTime.Today;
            vc_item.ma_gd = "1";
            vc_item.loai_ct = "1";
            vc_item.t_tien = vc_item.t_tien_nt;

            vc_item.ma_cuahang = Startup.Shop;
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
                    List<PXWDetail>? detail_list = JsonSerializer.Deserialize<List<PXWDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        //cập nhật ngày chứng từ
                        detail_list.ForEach(x => x.ngay_ct = vc_item.ngay_ct);
                        detail_list.ForEach(x => x.ma_cuahang = vc_item.ma_cuahang);
                        detail_list.ForEach(x => x.ma_ca = vc_item.ma_ca);


                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(PXWDetail).Name;
                }
            }
            result_model = checkImeiInsert(vc_item);
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
                query += $"insert into {detail_table} (stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du, tk_vt,so_luong,gia_nt,gia,tien_nt,tien, px_gia_dd, stt_rec_pn, stt_rec0pn,stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_imei) select stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du, tk_vt,so_luong,gia_nt,gia_nt,tien_nt,tien_nt, px_gia_dd, stt_rec_pn, stt_rec0pn,stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_imei from @{_DETAIL_PARA}";
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

            //update các trường null
            query = $"exec fs_UpdateNullToTable '{prime_table}', '{prime_table}', 'stt_rec = ''{stt_rec}''' \n";
            query += $"exec fs_UpdateNullToTable '{detail_table}', '{detail_table}', 'stt_rec = ''{stt_rec}''' \n";
            service.ExecuteNonQuery(query);

            //insert bảng master (c) & inquiry (i)
            string inquiry_table = this.InquiryTable.Trim() + expression;
            query = $"exec MokaOnline$App$Voucher$UpdateInquiryTable '{this.VoucherCode}', '{inquiry_table}', '{prime_table}', '{detail_table}', 'stt_rec', '{stt_rec}', '{this.Operation}' \n";
            query += $"exec MokaOnline$App$Voucher$UpdateGrandTable '{this.VoucherCode}', '{this.MasterTable}', '{prime_table}', 'stt_rec', '{stt_rec}'";
            service.ExecuteNonQuery(query);

            List<ImeiItem> list_imei = new List<ImeiItem>();
            // id = 1 ==> type: PVDetail
            int index_value = 1;
            if (vc_item.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_detail != null)
                {
                    List<PXWDetail> detail_list = item_detail.Data.Cast<PXWDetail>().ToList();
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        foreach (var item in detail_list)
                        {
                            List<string> imei = item.ma_imei.Split(',').ToList();
                            foreach (var imei_item in imei)
                            {
                                list_imei.Add(new ImeiItem
                                {
                                    ma_imei = imei_item.Trim(),
                                    ma_vt = item.ma_vt,
                                    ma_kho = item.ma_kho,
                                    gia_nt0 = item.gia_nt,
                                    ghi_chu = vc_item.dien_giai
                                });
                            }
                        }
                    }
                }
            }
            if (vc_item.status == "0")
            {
                string json = JsonSerializer.Serialize(list_imei);
                //create query insert IMEI
                query = $"exec Genbyte$IMEI$UpdateState$Inventory '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', 1, '{json}'";
                service.ExecuteNonQuery(query);
            }
            if (vc_item.status == "2")
            {
                query = $"EXEC Genbyte$IMEI$PXW$Update '{stt_rec}'";
                service.ExecuteNonQuery(query);
            }
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
            // Lấy danh sách tất cả các imei
            List<string> imeis = new List<string>();
            if (data.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                DetailItemModel? item_model = data.details.FirstOrDefault(x => x.Id == index_value);
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_model != null && item_detail != null)
                {
                    List<PXWDetail>? detail_list = JsonSerializer.Deserialize<List<PXWDetail>>((JsonElement)item_model.Data);
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        detail_list.ForEach((item) =>
                        {
                            item.tien = item.tien_nt;
                            if (item.ma_imei != null && item.ma_imei != "")
                            {
                                imeis.AddRange(item.ma_imei.Split(",").ToList().Select(x => x.Trim()));
                            }
                        });
                        item_detail.Data = new List<DetailEntity>();
                        item_detail.Data.AddRange(detail_list);
                    }
                    item_detail.Detail_Type = typeof(PXWDetail).Name;
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
            if (vc_item.status == "2")
            {
                var imeiService = new Imei.Service();
                List<Imei.ImeiState> state_imei = imeiService.GetStateOfImeis(imeis);
                Imei.ImeiState? exists = state_imei.FirstOrDefault(x => x.exists_yn == false);
                if (exists != null)
                {
                    result_model.success = false;
                    result_model.message = "imei_not_exists";
                    return result_model;
                }
            }

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
                query += $"insert into {detail_table} (stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du, tk_vt,so_luong,gia_nt,gia,tien_nt,tien, px_gia_dd, stt_rec_pn, stt_rec0pn,stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_imei) select stt_rec,stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_bp,so_lsx,dvt,he_so,ma_kho,ma_vi_tri,ma_lo,ma_vv, ma_nx, tk_du, tk_vt,so_luong,gia_nt,gia_nt,tien_nt,tien_nt, px_gia_dd, stt_rec_pn, stt_rec0pn,stt_rec_yc, stt_rec0yc, line_nbr,ma_hd,ma_ku,ma_phi,so_dh_i,ma_td1,ma_td2,ma_td3,sl_td1,sl_td2,sl_td3,ngay_td1,ngay_td2,ngay_td3,gc_td1,gc_td2,gc_td3,s1,ma_ca,ma_cuahang,s4,s5,s6,s7,s8,s9,ma_imei from @{_DETAIL_PARA}";
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

            string queryIMEI = "";
            if (list_imei_delete.Count > 0)
            {
                string json_del = JsonSerializer.Serialize(list_imei_delete);
                queryIMEI = $"exec Genbyte$IMEI$UpdateState$Inventory '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', 0, '{json_del}'";
                service.ExecuteNonQuery(queryIMEI);
            }
            List<ImeiItem> list_imei = new List<ImeiItem>();
            // id = 1 ==> type: PVDetail
            int index_value = 1;
            if (vc_item.details.Any(x => x.Id == index_value) && vc_item.details.Any(x => x.Id == index_value))
            {
                VoucherDetail? item_detail = vc_item.details.FirstOrDefault(x => x.Id == index_value);

                if (item_detail != null)
                {
                    List<PXWDetail> detail_list = item_detail.Data.Cast<PXWDetail>().ToList();
                    if (detail_list != null && detail_list.Count > 0)
                    {
                        foreach (var item in detail_list)
                        {
                            List<string> imei = item.ma_imei.Split(',').ToList();
                            foreach (var imei_item in imei)
                            {
                                list_imei.Add(new ImeiItem
                                {
                                    ma_imei = imei_item.Trim(),
                                    ma_vt = item.ma_vt,
                                    ma_kho = item.ma_kho,
                                    gia_nt0 = item.gia_nt,
                                    ghi_chu = vc_item.dien_giai
                                });
                            }
                        }
                    }
                }
            }
            if (vc_item.status == "0")
            {
                string json = JsonSerializer.Serialize(list_imei);
                //create query insert IMEI
                queryIMEI = $"exec Genbyte$IMEI$UpdateState$Inventory '{user_id}', '{vc_item.ma_cuahang}', '{stt_rec}', '{vc_item.ngay_ct?.ToString("yyyy-MM-dd")}', 1, '{json}'";
                service.ExecuteNonQuery(queryIMEI);
            }
            if (vc_item.status == "2")
            {
                query = $"EXEC Genbyte$IMEI$PXW$Update '{stt_rec}'";
                service.ExecuteNonQuery(query);
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
            sql = $"exec fs_Voucher$RemoveInv$Imei '{voucherId.Replace("'", "''")}', '{this.VoucherCode}' \n";
            sql += $"delete from {this.MasterTable} where stt_rec = @vc_id \n";
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
	SELECT @q = 'select a.*, b.ten_kh, b.dia_chi, c.ten_kh as ten_ongba, d.ten_bp from {1}' + @exp + ' a left join dmkh b on a.ma_kh = b.ma_kh left join dmkh c on a.ong_ba = c.ma_kh left join dmbp d on a.ma_bp = d.ma_bp where stt_rec = @stt_rec '
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
                IList<PXWDetail> pr_detail = ds.Tables[1].ToList<PXWDetail>();

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
                        var PRDetail = item as PXWDetail;
                        if (PRDetail != null && !string.IsNullOrEmpty(PRDetail.ma_imei))
                        {
                            listImei.Add(PRDetail.ma_imei.Trim());
                            ma_cuahang = PRDetail.ma_cuahang;
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
        #endregion
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
                        var svDetail = item as PXWDetail;
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
                        var svDetail = item as PXWDetail;
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

                    foreach (var item in item_detail.Data as List<PXWDetail>)
                    {
                        var svDetail = item as PXWDetail;
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
            return result_model;
        }
    }
}
