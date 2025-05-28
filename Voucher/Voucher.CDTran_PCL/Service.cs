using Genbyte.Base.CoreLib;
using Genbyte.Component.Voucher;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using Voucher.CDTran_PCL.Models;
using System.Text.Json;

namespace Voucher.CDTran_PCL
{
    public class Service : IVoucherService
    {
        //Mã chứng từ
        public string VoucherCode { get; } = "PCL";

        //Bảng gốc dữ liệu không phân kỳ
        public string MasterTable { get; } = "c558$000000";

        //Bảng chính chứa dữ liệu phân kỳ
        public string PrimeTable { get; } = "m558$";

        //Bảng lưu thông tin tìm kiếm
        public string InquiryTable { get; } = "i558$";

        //Bảng lưu dữ liệu chi tiết của chứng từ
        public string DetailTable { get; } = "d558$";
        private const string _DETAIL_PARA = "d558";

        //Chuỗi format phục vụ tạo dữ liệu tại bảng inquiry
        public string Operation { get; } = "ma_kh,ma_dvcs,ma_cuahang,ma_ca;#10$,#20$,#30$, #40$; , , , :;;";

        /// <summary>
        /// Chuỗi truy vấn khi load chứng từ
        /// </summary>
        public string LoadingQuery { get; } = "exec MokaOnline$App$Voucher$Loading '@@VOUCHER_CODE', '@@MASTER_TABLE', '@@PRIME_TABLE', 'ngay_ct', 'convert(char(6), {0}, 112)', '000000', 0, 'stt_rec', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(ma_ca) as ma_ca,ngay_ct,rtrim(so_ct) as so_ct,rtrim(ma_kh) as ma_kh,rtrim(ma_cuahang) as ma_cuahang,rtrim(Dien_giai) as dien_giai,t_tien_nt,t_tt_nt,rtrim(ma_nt) as ma_nt,rtrim(ma_ct) as ma_ct,rtrim(status) as status,rtrim(user_id0) as user_id0,rtrim(user_id2) as user_id2,datetime0,datetime2', 'rtrim(stt_rec) as stt_rec,rtrim(ma_dvcs) as ma_dvcs,rtrim(a.ma_ca) as ma_ca,c.ten_ca, rtrim(a.ma_cuahang) as ma_cuahang, ngay_ct,rtrim(so_ct) as so_ct,rtrim(a.ma_kh) as ma_kh,b.ten_kh,rtrim(a.Dien_giai) as dien_giai,t_tien_nt,t_tt_nt,rtrim(ma_nt) as ma_nt,rtrim(a.ma_ct) as ma_ct,rtrim(a.status) as status,rtrim(a.user_id0) as user_id0,rtrim(a.user_id2) as user_id2,a.datetime0,a.datetime2,x.statusname,y.comment,z.comment2,'''' as Hash', 'a left join vdmkh_acc b on a.ma_kh = b.ma_kh left join dmca c on a.ma_ca = c.ma_ca left join dmttct x on a.status = x.status and a.ma_ct = x.ma_ct left join @@SYSDATABASE..userinfo y on a.user_id0 = y.id left join @@SYSDATABASE..userinfo z on a.user_id2 = z.id', '@@ORDER_BY', @@ADMIN, @@USER_ID, 1, 0, '', '', 'ma_cuahang = ''" + Startup.Shop + "'''";

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
            VoucherRight.AllowReadAll = true;
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
            return result_model;
        }
        #endregion

        #region Inserted
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

            // cập nhật ngay_ct là ngày hiện tại khi phiếu hoàn thành
            if(vc_item.status == "2")
            {
                vc_item.ngay_ct = DateTime.Today;
            }

            //convert dữ liệu chi tiết chứng từ
            // id = 1 ==> type: PCLDetail
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
                                List<PCLDetail>? detail_list = JsonSerializer.Deserialize<List<PCLDetail>>((JsonElement)item_model.Data);
                                if (detail_list != null && detail_list.Count > 0)
                                {
                                    //cập nhật ngày chứng từ
                                    detail_list.ForEach(x => {
                                        x.ngay_ct = vc_item.ngay_ct;
                                    });
                                    item_detail.Data = new List<DetailEntity>();
                                    item_detail.Data.AddRange(detail_list);
                                }
                                item_detail.Detail_Type = typeof(PCLDetail).Name;
                                break;
                            default:
                                break;
                        }

                    }
                }
                index_value++;
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

            /*
            IF NOT EXISTS(SELECT 1 FROM dmttct WHERE (xdefault = 1 OR right_yn = 1) AND ma_ct = @vc_code AND status = @vc_status) BEGIN
	             UPDATE @check SET is_success = 0, message = 'status_cannot_update'
	             SELECT * FROM @check
	             RETURN
            END
            */

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
                vc_item.ma_gd = old_voucher.ma_gd;
                vc_item.ma_loaigd = old_voucher.ma_loaigd;
                vc_item.loai_ct = old_voucher.loai_ct;
                vc_item.t_tien = vc_item.t_tien_nt;
                vc_item.t_tt = vc_item.t_tt_nt;

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
                query += $"insert into {detail_table} (stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, dien_giai, tk_no, tien_nt, tien, ma_kh_i, ma_vv, ma_sp, ma_bp, so_lsx, tt_qd, stt_rec_tt, ma_thue, tk_thue, thue_suat, loai_hd, thue, thue_nt, tt, tt_nt, ngay_ct0, so_ct0, so_seri0, mau_bc, ma_tc, ma_kh, ten_kh, dia_chi, ma_so_thue, ma_kh2, ten_vt, ghi_chu, ty_gia_ht2, tien_ht_nt, tien_ht, line_nbr, ma_hd, ma_ku, ma_phi, so_dh_i, ma_td1, ma_td2, ma_td3, sl_td1, sl_td2, sl_td3, ngay_td1, ngay_td2, ngay_td3, gc_td1, gc_td2, gc_td3, s1, ma_ca, ma_cuahang, s4, s5, s6, s7, s8, s9) select stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, dien_giai, tk_no, tien_nt, tien, ma_kh_i, ma_vv, ma_sp, ma_bp, so_lsx, tt_qd, stt_rec_tt, ma_thue, tk_thue, thue_suat, loai_hd, thue, thue_nt, tt, tt_nt, ngay_ct0, so_ct0, so_seri0, mau_bc, ma_tc, ma_kh, ten_kh, dia_chi, ma_so_thue, ma_kh2, ten_vt, ghi_chu, ty_gia_ht2, tien_ht_nt, tien_ht, line_nbr, ma_hd, ma_ku, ma_phi, so_dh_i, ma_td1, ma_td2, ma_td3, sl_td1, sl_td2, sl_td3, ngay_td1, ngay_td2, ngay_td3, gc_td1, gc_td2, gc_td3, s1, ma_ca, ma_cuahang, s4, s5, s6, s7, s8, s9 from @{_DETAIL_PARA}";
            }
            query += "\n\n";

            //cập nhật ngày chứng từ tự động lấy mặc định là ngày hệ thống
            // new table
            DateTime new_ngay_ct = DateTime.Today;
            string new_partition = new_ngay_ct.ToString("yyyyMM");
            string new_prime_table = this.PrimeTable.Trim() + new_partition;
            string new_detail_table = this.DetailTable.Trim() + new_partition;
            string new_inquiry_table = this.InquiryTable.Trim() + new_partition;
            // old table
            string old_prime_table = prime_table;
            string old_detail_table = detail_table;
            string old_inquiry_table = this.InquiryTable.Trim() + expression;
            // query
            query += "\n";
            query += $"declare @today DATETIME = '{new_ngay_ct.ToString("yyyy-MM-dd")}' \n";
            query += $"declare @old_partition char(6) = '{expression}', @new_partition char(6) = '{new_partition}' \n";
            query += @$"if exists(select 1 from {old_prime_table} where stt_rec = @stt_rec and status <> '0' and ngay_ct <> @today) begin
	        SET XACT_ABORT ON
	            BEGIN TRAN
	            BEGIN TRY
		            if @old_partition <> @new_partition begin
			            select * into #tmp_prime from {old_prime_table} where stt_rec = @stt_rec
			            select * into #tmp_detail from {old_detail_table} where stt_rec = @stt_rec

			            update #tmp_prime set ngay_ct = @today where stt_rec = @stt_rec
			            update #tmp_detail set ngay_ct = @today where stt_rec = @stt_rec

			            delete from {old_prime_table} where stt_rec = @stt_rec
			            delete from {old_detail_table} where stt_rec = @stt_rec
			            delete from {old_inquiry_table} where stt_rec = @stt_rec

			            insert into {new_prime_table} select * from #tmp_prime
			            insert into {new_detail_table} select * from #tmp_detail

			            drop table #tmp_prime
			            drop table #tmp_detail
		            end
		            else begin
			            update {this.MasterTable} set ngay_ct = @today where stt_rec = @stt_rec
			            update {old_prime_table} set ngay_ct = @today where stt_rec = @stt_rec
			            update {old_detail_table} set ngay_ct = @today where stt_rec = @stt_rec
			            update {old_inquiry_table} set ngay_ct = @today where stt_rec = @stt_rec
		            end
		            COMMIT
	            END TRY
	            BEGIN CATCH
	               ROLLBACK
	               DECLARE @ErrorMessage VARCHAR(2000)
	               SELECT @ErrorMessage = ERROR_MESSAGE()
	               INSERT INTO log_syncerror (name, cr_date, message) VALUES('OPTran', GETDATE(), @ErrorMessage)
	               RAISERROR(@ErrorMessage, 16, 1)
	            END CATCH
	            SET XACT_ABORT OFF
            end";
            query += "\n\n";

            //set lại tên bảng theo phân kỳ mới nếu khác ngay_ct
            if (expression != new_partition)
            {
                prime_table = new_prime_table;
                detail_table = new_detail_table;
                expression = new_partition;
            }

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
	                SELECT @q = 'select a.*, b.ten_kh from {1}' + @exp + ' a left join vdmkh_acc b on a.ma_kh = b.ma_kh where stt_rec = @stt_rec '
	                SELECT @q = @q + CHAR(13) + 'select a.*, b.ten_phi from {2}' + @exp + ' a left join dmphi b on b.ma_phi = a.ma_phi where a.stt_rec = @stt_rec'
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
                IList<PCLDetail> pr_detail = ds.Tables[1].ToList<PCLDetail>();

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
            return new List<ImeiState>();
        }
        #endregion
    }
}
