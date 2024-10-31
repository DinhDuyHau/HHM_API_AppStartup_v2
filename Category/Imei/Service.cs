using Genbyte.Base.CoreLib;
using Genbyte.Base.Security;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Imei.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Imei
{
    public class Service : CoreService
    {
        private readonly IConfiguration _configuration;
        public Service()
        {
        }
        public Service(IConfiguration _configuration) {
            this._configuration = _configuration;
        }

        #region Check SQL Injection
        /// <summary>
        /// Viết mới lại phương thức kiểm tra SQL injection cho lớp dẫn xuất
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public new bool IsSQLInjectionValid(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (!CheckSQLInjectionForString(array[i])) return false;
            }
            return true;
        }

        public static bool CheckSQLInjectionForString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
            string[] strArray2 = new string[] {
                "'",
                "--",
                ";",
                "alter",
                "drop",
                "insert",
                "update",
                "delete",
                "exec",
                "create",
                "truncate"
            };
            int num3 = (strArray2.Length - 1);
            int i = 0;
            while ((i <= num3))
            {
                if ((value.IndexOf(strArray2[i], StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return false;
                }
                i += 1;
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Lấy trạng thái tồn tức thời của imei, các trạng thái và thông tin chi tiết imei
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="shop_id"></param>
        /// <param name="vc_code"></param>
        /// <returns></returns>
        #region GetImeiInStore
        public DataSet GetImeiInStore(string imei, string shop_id, string vc_code, string ma_kh)
        {
            string sql = "exec Genbyte$IMEI$GetImeiInfo @imei, @shop_id, @vc_code, @ma_kh";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@imei",
                SqlDbType = SqlDbType.Char,
                Value = imei
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = shop_id
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@vc_code",
                SqlDbType = SqlDbType.Char,
                Value = vc_code
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_kh",
                SqlDbType = SqlDbType.Char,
                Value = ma_kh == null? DBNull.Value : ma_kh
            });
            return base.ExecSql2DataSet(sql, paras);
        }
        #endregion

        /// <summary>
        /// Lấy trạng thái tồn tức thời của imei, các trạng thái và thông tin chi tiết imei trong chứng từ thu cũ
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="shop_id"></param>
        /// <param name="vc_code"></param>
        /// <returns></returns>
        #region GetPriceRenew
        public DataSet GetPriceRenew(RenewModel renew)
        {
            string sql = "exec Genbyte$IMEI$GetImeiInfoRenew @imei, @shop_id, @ma_ncc, @list_vt, @imei_thu_cu, @ngay_ct, @tong_tien_ht, @tien_thu_cu";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@imei",
                SqlDbType = SqlDbType.Char,
                Value = renew.ma_imei
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = renew.ma_cuahang
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_ncc",
                SqlDbType = SqlDbType.Char,
                Value = renew.ma_ncc == null ? DBNull.Value : renew.ma_ncc
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@list_vt",
                SqlDbType = SqlDbType.Char,
                Value = renew.list_vt == null ? DBNull.Value : string.Join(",", renew.list_vt)
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@imei_thu_cu",
                SqlDbType = SqlDbType.VarChar,
                Value = renew.imei_thu_cu == null ? "" : renew.imei_thu_cu
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = renew.ngay_ct == null ? DBNull.Value : renew.ngay_ct.Value
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@tong_tien_ht",
                SqlDbType = SqlDbType.Decimal,
                Value = renew.tong_tien_ht.HasValue ? renew.tong_tien_ht : 0
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@tien_thu_cu",
                SqlDbType = SqlDbType.Decimal,
                Value = renew.tien_thu_cu.HasValue ? renew.tien_thu_cu : 0
            });
            return base.ExecSql2DataSet(sql, paras);
        }
        #endregion

        /// <summary>
        /// Lấy thông tin tình trạng của danh sách imei
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        #region GetStateOfImeis
        public List<ImeiState> GetStateOfImeis(List<string> imeis)
        {
            string imei_list = string.Join(",", imeis);
            string sql = "exec Genbyte$IMEI$GetState @shop_id, @imeis";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = Startup.Shop == null ? string.Empty : Startup.Shop
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@imeis",
                SqlDbType = SqlDbType.Char,
                Value = imei_list
            });
            return base.ExecSql2List<ImeiState>(sql, paras);
        }
        #endregion

        /// <summary>
        /// Lấy thông tin tình trạng của danh sách imei
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        #region GetStateAndItemOfImeis
        public List<ImeiInfo> GetStateAndItemOfImeis(List<string> imeis)
        {
            string imei_list = string.Join(",", imeis);
            string sql = "exec Genbyte$IMEI$GetStateAndItem @shop_id, @imeis";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = Startup.Shop == null ? string.Empty : Startup.Shop
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@imeis",
                SqlDbType = SqlDbType.Char,
                Value = imei_list
            });
            return base.ExecSql2List<ImeiInfo>(sql, paras);
        }
        #endregion

        /// <summary>
        /// Lấy thông tin tình trạng của danh sách imei và trạng thái tồn trong kho chỉ định
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        #region GetStateAndItemOfImeis
        public List<ImeiInfo> GetStateItemOfImeisInStock(List<string> imeis, string ma_kho)
        {
            string imei_list = string.Join(",", imeis);
            string sql = "exec Genbyte$IMEI$GetStateAndItem @shop_id, @imeis, @ma_kho";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = Startup.Shop == null ? string.Empty : Startup.Shop
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@imeis",
                SqlDbType = SqlDbType.Char,
                Value = imei_list
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_kho",
                SqlDbType = SqlDbType.Char,
                Value = ma_kho
            });
            return base.ExecSql2List<ImeiInfo>(sql, paras);
        }
        #endregion

        /// <summary>
        /// Lấy danh sách imei tồn kho tại cửa hàng theo mã vật tư
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        #region GetImeisInStoreByItem
        public EntityCollection<Dictionary<string, object>> GetImeisInStoreByItem(string shop_id, string voucher_code, string ma_vt = "", string ten_vt = "", string ma_imei = "", string ma_kho = "", int page_index = 1, int page_size = 0)
        {
            string sql = "exec Genbyte$IMEI$GetByItem @shop_id, @vc_code, @item, @item_name, @imei, @site, @page_index, @page_size";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = shop_id
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@vc_code",
                SqlDbType = SqlDbType.Char,
                Value = voucher_code
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@item",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@item_name",
                SqlDbType = SqlDbType.NVarChar,
                Value = ten_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@imei",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_imei
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@site",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kho
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@page_index",
                SqlDbType = SqlDbType.Int,
                Value = page_index
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@page_size",
                SqlDbType = SqlDbType.Int,
                Value = page_size
            });
            DataSet ds = base.ExecSql2DataSet(sql, paras);

            if (ds == null || ds.Tables.Count <= 0)
                return null;
            EntityCollection<Dictionary<string, object>> entities = new EntityCollection<Dictionary<string, object>>();

            //paging information (lấy từ bảng đầu tiên trong danh sách dataset)
            if (ds.Tables.Count >= 2)
            {
                if (ds.Tables[0].Columns.Contains("TotalPage"))
                    entities.PageCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalPage"]);
                if (ds.Tables[0].Columns.Contains("PageSize"))
                    entities.PageSize = Convert.ToInt32(ds.Tables[0].Rows[0]["PageSize"]);
                if (ds.Tables[0].Columns.Contains("TotalRecordCount"))
                    entities.RecordCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRecordCount"]);

                entities.Items = Converter.TableToDictionary(ds.Tables[1]);
            }

            return entities;
        }
        #endregion

        /// <summary>
        /// Tìm lọc danh sách imei theo key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        #region FindImeisByKey
        public List<Dictionary<string, object>> FindImeisByKey(string shop_id, string key)
        {
            string sql = "exec Genbyte$IMEI$FindByKey @shop_id, @key";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = shop_id
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@key",
                SqlDbType = SqlDbType.VarChar,
                Value = key
            });
            return base.ExecSql2Dictionary(sql, paras);
        }
        #endregion

        /// <summary>
        /// Cập nhật trạng thái đặt hàng cho imei
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="imeis"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        #region UpdateImeiOrderState
        public List<ImeiState> UpdateImeiOrderState(string shop_id, List<string> imeis, bool state, string? ma_ct, int? nxt)
        {
            string imei_list = string.Join(",", imeis);
            if (nxt is null)
            {
                nxt = 2;
            }
            string sql = "exec Genbyte$IMEI$UpdateState @shop_id, @imeis, @state, @nxt, @ma_ct";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@shop_id",
                SqlDbType = SqlDbType.Char,
                Value = shop_id
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@imeis",
                SqlDbType = SqlDbType.VarChar,
                Value = imei_list
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@state",
                SqlDbType = SqlDbType.Bit,
                Value = state
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@nxt",
                SqlDbType = SqlDbType.Int,
                Value = nxt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ct",
                SqlDbType = SqlDbType.Char,
                Value = ma_ct == null ? "" : ma_ct
            });
            return base.ExecSql2List<ImeiState>(sql, paras);
        }
        #endregion

        /// <summary>
        /// Lấy danh sách hàng khuyến mại theo imei
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        #region GetPromotionByImei
        public List<Dictionary<string, object>> GetPromotionByImei(string shop_id, string ma_imei)
        {
            string sql = "exec fs_Calc$Discount$PromotionItems @shop_id, @ma_imei";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@shop_id",
                SqlDbType = SqlDbType.VarChar,
                Value = shop_id
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_imei",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_imei
            });
            return base.ExecSql2Dictionary(sql, paras);
        }
        #endregion

        /// <summary>
        /// GetImeiSoldInfo
        /// </summary>
        /// <param name="imeiId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        #region GetImeiSoldInfo
        public CommonObjectModel GetImeiSoldInfo(string imeiId, string ma_cuahang, string ma_ct, decimal rate, decimal tien_giam, string loai_tra_lai = "", bool tra_lai_cod = false)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };
            CoreService core_service = new CoreService();

            //Lấy dữ liệu từ bảng prime và detail theo id truyền vào
            string sql = @"exec Genbyte$IMEI$GetSoldInfo @ma_imei, @ma_cuahang, @ma_ct, @rate, @tien_giam, @loai_tra_lai, @tra_lai_cod";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
            new SqlParameter()
            {
                ParameterName = "@ma_imei",
                SqlDbType = SqlDbType.NVarChar,
                Value = imeiId.Trim()
            },new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.NVarChar,
                Value = ma_cuahang.Trim()
            },new SqlParameter()
            {
                ParameterName = "@ma_ct",
                SqlDbType = SqlDbType.NVarChar,
                Value = ma_ct.Trim()
            }
            ,new SqlParameter()
            {
                ParameterName = "@rate",
                SqlDbType = SqlDbType.Decimal,
                Value = rate
            }
            ,new SqlParameter()
            {
                ParameterName = "@tien_giam",
                SqlDbType = SqlDbType.Decimal,
                Value = tien_giam
            },
            new SqlParameter()
            {
                ParameterName = "@loai_tra_lai",
                SqlDbType = SqlDbType.Char,
                Value = loai_tra_lai.Trim()
            },
            new SqlParameter()
            {
                ParameterName = "@tra_lai_cod",
                SqlDbType = SqlDbType.Bit,
                Value = tra_lai_cod
            }
            });
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);

            if (ds != null && ds.Tables.Count >= 1)
            {
                VoucherItem vc_item = ds.Tables[0].ToList<VoucherItem>().FirstOrDefault();
                IList<SVDetail> pr_detail = ds.Tables[1].ToList<SVDetail>();
                IList<DVDetail> dv_detail = ds.Tables[2].ToList<DVDetail>();
                IList<CKDetail> ck_detail = ds.Tables[3].ToList<CKDetail>();
                IList<TTDetail> tt_detail = ds.Tables[4].ToList<TTDetail>();
                IList<BHDetail> bh_detail = ds.Tables[5].ToList<BHDetail>();
                IList<KMDetail> km_detail = ds.Tables[6].ToList<KMDetail>();
                IList<EInvoiceInfo> einvoice_info = ds.Tables[7].ToList<EInvoiceInfo>();

                vc_item.stt_rec = APIService.EncryptForWebApp(vc_item.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                pr_detail.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                    v.stt_rec_ct = !string.IsNullOrWhiteSpace(v.stt_rec_ct) ? APIService.EncryptForWebApp(v.stt_rec_ct, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_dh = !string.IsNullOrWhiteSpace(v.stt_rec_dh) ? APIService.EncryptForWebApp(v.stt_rec_dh, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_gh = !string.IsNullOrWhiteSpace(v.stt_rec_gh) ? APIService.EncryptForWebApp(v.stt_rec_gh, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_pn = !string.IsNullOrWhiteSpace(v.stt_rec_pn) ? APIService.EncryptForWebApp(v.stt_rec_pn, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_px = !string.IsNullOrWhiteSpace(v.stt_rec_px) ? APIService.EncryptForWebApp(v.stt_rec_px, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                });

                dv_detail.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                    v.stt_rec_ct = !string.IsNullOrWhiteSpace(v.stt_rec_ct) ? APIService.EncryptForWebApp(v.stt_rec_ct, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_dh = !string.IsNullOrWhiteSpace(v.stt_rec_dh) ? APIService.EncryptForWebApp(v.stt_rec_dh, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_gh = !string.IsNullOrWhiteSpace(v.stt_rec_gh) ? APIService.EncryptForWebApp(v.stt_rec_gh, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_pn = !string.IsNullOrWhiteSpace(v.stt_rec_pn) ? APIService.EncryptForWebApp(v.stt_rec_pn, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                    v.stt_rec_px = !string.IsNullOrWhiteSpace(v.stt_rec_px) ? APIService.EncryptForWebApp(v.stt_rec_px, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                });

                ck_detail.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                });

                tt_detail.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                });
                bh_detail.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                });
                km_detail.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                    v.stt_rec_tq = !string.IsNullOrWhiteSpace(v.stt_rec_tq) ? APIService.EncryptForWebApp(v.stt_rec_tq, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]) : "";
                });

                einvoice_info.ToList().ForEach(v =>
                {
                    v.stt_rec = APIService.EncryptForWebApp(v.stt_rec, _configuration["Security:KeyAES"], _configuration["Security:IVAES"]);
                });

                BaseModel invoice_model = new BaseModel();
                invoice_model.masterInfo = vc_item;
                invoice_model.details = new List<DetailItemModel>();
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 1,
                    Name = "Items",
                    Data = pr_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 2,
                    Name = "Services",
                    Data = dv_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 3,
                    Name = "Discounts",
                    Data = ck_detail
                });

                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 4,
                    Name = "Payments",
                    Data = tt_detail
                });

                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 5,
                    Name = "Warranty",
                    Data = bh_detail
                });

                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 6,
                    Name = "Ext",
                    Data = km_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 7,
                    Name = "Einvoice",
                    Data = einvoice_info
                });
                model.result = invoice_model;
            }
            return model;
        }
        #endregion


        /// <summary>
        /// Lấy thông tin bảo hành của imei bán ra
        /// </summary>
        /// <param name="imeiId"></param>
        /// <param name="ma_cuahang"></param>
        /// <returns></returns>
        #region GetImeiWarrantyOutInfo
        public List<ImeiWarrantyOut> GetImeiWarrantyOutInfo(string imeiId, string ma_cuahang)
        {
            CoreService core_service = new CoreService();

            string sql = @"exec Genbyte$Imei$CheckWarrantyOut @ma_imei, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
                new SqlParameter()
                {
                    ParameterName = "@ma_imei",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = imeiId.Trim()
                },new SqlParameter()
                {
                    ParameterName = "@ma_cuahang",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = ma_cuahang.Trim()
                }
            });
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);
            return ds != null && ds.Tables.Count >= 1 && ds.Tables[0].Rows.Count > 0 ? 
                ds.Tables[0].ToList<ImeiWarrantyOut>().ToList() : null;
        }
        #endregion

        /// <summary>
        /// Lấy danh sách tất cả hàng tặng của imei
        /// </summary>
        /// <param name="imeiId"></param>
        /// <param name="ma_ck"></param>
        /// <param name="rec"></param>
        /// <returns></returns>
        public List<GiftItem> GetAllGiftPromotionsForImei(string imeiId, string ma_ck, int rec)
        {
            CoreService core_service = new CoreService();

            string sql = @"exec Genbyte$IMEI$GiftPromotionsForImei @ma_imei, @ma_ck, @rec, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
                new SqlParameter()
                {
                    ParameterName = "@ma_imei",
                    SqlDbType = SqlDbType.VarChar,
                    Value = imeiId.Trim()
                },
                new SqlParameter()
                {
                    ParameterName = "@ma_ck",
                    SqlDbType = SqlDbType.VarChar,
                    Value = ma_ck.Trim()
                },
                new SqlParameter()
                {
                    ParameterName = "@rec",
                    SqlDbType = SqlDbType.Int,
                    Value = rec
                },
                new SqlParameter()
                {
                    ParameterName = "@ma_cuahang",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Startup.Shop
                }
            });
            List<GiftItem> gifts = core_service.ExecSql2List<GiftItem>(sql, paras);
            return gifts;
        }

        /// <summary>
        /// Tìm kiếm imei bảo hành theo imei và mã cửa hàng
        /// </summary>
        /// <param name="imeiId"></param>
        /// <param name="ma_cuahang"></param>
        /// <returns></returns>
        #region SearchImeiWarranty
        public List<ImeiWarrantyOut> SearchImeiWarranty(string imeiId, string ma_cuahang)
        {
            CoreService core_service = new CoreService();

            string sql = @"exec Genbyte$Imei$SearchImeiWarranty @ma_imei, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
                new SqlParameter()
                {
                    ParameterName = "@ma_imei",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = imeiId.Trim()
                },new SqlParameter()
                {
                    ParameterName = "@ma_cuahang",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = ma_cuahang.Trim()
                }
            });
            DataSet ds = core_service.ExecSql2DataSet(sql, paras);
            return ds != null && ds.Tables.Count >= 1 && ds.Tables[0].Rows.Count > 0 ?
                ds.Tables[0].ToList<ImeiWarrantyOut>().ToList() : null;
        }
        #endregion
    }
}