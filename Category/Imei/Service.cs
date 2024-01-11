using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Imei.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using System.Data.SqlClient;

namespace Imei
{
    public class Service : CoreService
    {
        /// <summary>
        /// Lấy trạng thái tồn tức thời của imei, các trạng thái và thông tin chi tiết imei
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="shop_id"></param>
        /// <param name="vc_code"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Lấy trạng thái tồn tức thời của imei, các trạng thái và thông tin chi tiết imei trong chứng từ thu cũ
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="shop_id"></param>
        /// <param name="vc_code"></param>
        /// <returns></returns>
        public DataSet GetPriceRenew(RenewModel renew)
        {
            string sql = "exec Genbyte$IMEI$GetImeiInfoRenew @imei, @shop_id, @ma_ncc, @list_vt";
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
            return base.ExecSql2DataSet(sql, paras);
        }

        /// <summary>
        /// Lấy thông tin tình trạng của danh sách imei
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Lấy thông tin tình trạng của danh sách imei
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Lấy danh sách imei tồn kho tại cửa hàng theo mã vật tư
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        public EntityCollection<Dictionary<string, object>> GetImeisInStoreByItem(string shop_id, string voucher_code, string ma_vt = "", int page_index = 1, int page_size = 0)
        {
            string sql = "exec Genbyte$IMEI$GetByItem @shop_id, @vc_code, @item, @page_index, @page_size";
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

        /// <summary>
        /// Tìm lọc danh sách imei theo key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Cập nhật trạng thái đặt hàng cho imei
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="imeis"></param>
        /// <param name="state"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Lấy danh sách hàng khuyến mại theo imei
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
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
        /// <summary>
        /// GetImeiSoldInfo
        /// </summary>
        /// <param name="imeiId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public CommonObjectModel GetImeiSoldInfo(string imeiId, string ma_cuahang, string ma_ct, decimal rate, decimal tien_giam)
        {
            CommonObjectModel model = new CommonObjectModel()
            {
                success = false,
                message = "",
                result = null
            };
            CoreService core_service = new CoreService();

            //Lấy dữ liệu từ bảng prime và detail theo id truyền vào
            string sql = @"exec Genbyte$IMEI$GetSoldInfo @ma_imei, @ma_cuahang, @ma_ct, @rate, @tien_giam";
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
            }});
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

                BaseModel invoice_model = new BaseModel();
                invoice_model.masterInfo = vc_item;
                invoice_model.details = new List<DetailItemModel>();
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 1,
                    Data = pr_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 2,
                    Data = dv_detail
                });
                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 3,
                    Data = ck_detail
                });

                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 4,
                    Data = tt_detail
                });

                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 5,
                    Data = bh_detail
                });

                invoice_model.details.Add(new DetailItemModel()
                {
                    Id = 6,
                    Data = km_detail
                });
                model.result = invoice_model;
            }
            return model;
        }
    }
}