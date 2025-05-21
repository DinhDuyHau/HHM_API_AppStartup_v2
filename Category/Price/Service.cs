using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Price.Model;

namespace Price
{
    public class Service : CoreService
    {
        public ServicePriceModel GetPriceOfServiceItem(string ma_vt, string ma_dichvu, string ma_cuahang, decimal gia_ban) 
        { 
            string sql = "exec Genbyte$Service$GetPrice @item_code, @service_id, @shop_id, @price";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@item_code",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@service_id",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_dichvu
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@shop_id",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@price",
                SqlDbType = SqlDbType.Decimal,
                Value = gia_ban
            });
            List<ServicePriceModel> entities = base.ExecSql2List<ServicePriceModel>(sql, paras);
            return entities.FirstOrDefault();
        }

        public List<RenewPriceModel> GetRenewPrice(string ma_vt, string ma_cuahang, string ma_ncc)
        {
            string sql = "exec Genbyte$Item$GetRenewPrice @ma_vt, @ma_cuahang, @ma_ncc";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_vt",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_ncc",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_ncc ?? ""
            });
            List<RenewPriceModel> entities = base.ExecSql2List<RenewPriceModel>(sql, paras);
            return entities;
        }

        public List<AdjustPriceModel> GetRenewAdjustBuyPrice(DateTime ngay_ct, string ma_cttc, string ma_ncc, string ma_loai, string ma_vt_mua, string ma_vt_ban, decimal gia_mua, decimal gia_dc, string ma_td3)
        {
            string sql = "exec Genbyte$Voucher$BHK$AdjustBuyPrice @ngay_ct, @ma_cttc, @ma_ncc, @ma_loai, @ma_vt_mua, @ma_vt_ban, @gia_mua, @gia_dc, @ma_td3";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_cttc",
                SqlDbType = SqlDbType.VarChar,
                Value = string.IsNullOrEmpty(ma_cttc) ? "" : ma_cttc
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ncc",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_ncc ?? ""
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_loai",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_loai
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_vt_mua",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt_mua
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_vt_ban",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt_ban
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@gia_mua",
                SqlDbType = SqlDbType.Decimal,
                Value = gia_mua
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@gia_dc",
                SqlDbType = SqlDbType.Decimal,
                Value = gia_dc
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_td3",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_td3
            });
            List<AdjustPriceModel> entities = base.ExecSql2List<AdjustPriceModel>(sql, paras);
            return entities;
        }

        public ReturnPriceModel GetReturnPrice(string ma_vt, string ma_imei, string ma_cuahang, DateTime ngay_ct, decimal gia_nt, decimal rate, decimal tien_giam)
        {
            string sql = "exec Genbyte$Item$GetReturnPrice @ma_vt, @ma_imei, @ma_cuahang, @ngay_ct, @gia_nt, @rate, @tien_giam";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_vt",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_imei",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_imei
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@gia_nt",
                SqlDbType = SqlDbType.Decimal,
                Value = gia_nt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@rate",
                SqlDbType = SqlDbType.Decimal,
                Value = ngay_ct
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@tien_giam",
                SqlDbType = SqlDbType.Decimal,
                Value = ngay_ct
            });
            List<ReturnPriceModel> entities = base.ExecSql2List<ReturnPriceModel>(sql, paras);
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Lấy thuế suất và mã thuế theo dịch vụ
        /// </summary>
        /// <param name="ma_dichvu"></param>
        /// <returns></returns>
        public ServiceBuyBackModel GetTaxOfServiceBuyback(string ma_dichvu)
        {
            string sql = @"SELECT a.ma_dv, a.ma_thue, b.thue_suat 
		                        FROM dmdichvu a 
		                        LEFT JOIN dmthue b ON a.ma_thue = b.ma_thue 
		                        WHERE ma_dv = @service_id";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@service_id",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_dichvu
            });
            List<ServiceBuyBackModel> entities = base.ExecSql2List<ServiceBuyBackModel>(sql, paras);
            return entities.FirstOrDefault();
        }


        /// <summary>
        /// Lấy khoảng giá được phép điều chỉnh
        /// </summary>
        /// <param name="ma_dichvu"></param>
        /// <returns></returns>
        public List<RepurchaseAdjustPriceModel> GetRepurchaseAdjustBuyPrice(DateTime ngay_ct, string ma_ncc, string ma_loai, string ma_vt_mua, decimal gia_mua, decimal gia_dc)
        {
            string sql = "exec Genbyte$Price$AdjustRangePrice @ngay_ct, @ma_ncc, @ma_loai, @ma_vt_mua, @gia_mua, @gia_dc";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ncc",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_ncc ?? ""
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_loai",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_loai
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_vt_mua",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt_mua
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@gia_mua",
                SqlDbType = SqlDbType.Decimal,
                Value = gia_mua
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@gia_dc",
                SqlDbType = SqlDbType.Decimal,
                Value = gia_dc
            });
            List<RepurchaseAdjustPriceModel> entities = base.ExecSql2List<RepurchaseAdjustPriceModel>(sql, paras);
            return entities;
        }

        /// <summary>
        /// Lấy chương trình thu cũ
        /// </summary>
        /// <param name="ma_ncc">mã nhà cung cấp</param>
        /// <param name="ngay_ct">ngày chứng từ</param>
        /// <returns></returns>
        public OldProgram GetOldProgram(string ma_ncc, DateTime ngay_ct)
        {
            string sql = @"SELECT TOP 1 * FROM dmctthucu WHERE ma_ncc = @ma_ncc AND @ngay_ct BETWEEN ngay_bd AND ngay_kt ORDER BY ngay_bd DESC";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ncc",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_ncc ?? ""
            });
            DataSet dataSet = base.ExecSql2DataSet(sql, paras);
                
            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                DataRow row = dataSet.Tables[0].Rows[0];
                return new OldProgram
                {
                    ma_cttc = row[0].ToString().Trim(),
                    ten_cttc = row[1].ToString().Trim(),
                    ma_ncc = row[2].ToString().Trim(),
                    ngay_bd = Convert.ToDateTime(row[3]),
                    ngay_kt = Convert.ToDateTime(row[4]),
                };
            }

            return null;
        }

        /// <summary>
        /// Lấy giá trị loai_kho_nhap từ bảng dmkbht_hangcu
        /// </summary>
        /// <param name="ma_cttc">Mã chương trình thu cũ</param>
        /// <param name="ma_kh">Mã khách hàng</param>
        /// <param name="ngay_ct">Ngày chứng từ</param>
        /// <returns></returns>
        public Dictionary<string, string> GetTypeStock(string ma_cttc, string ma_kh, DateTime ngay_ct)
        {
            string sql = @"
                SELECT TOP 1 a.loai_kho_nhap, b.ten_loai 
                FROM dmkbht_hangcu a 
                LEFT JOIN dmloaikho b ON b.ma_loai = a.loai_kho_nhap
                WHERE a.ma_cttc = @ma_cttc 
                AND a.ma_kh = @ma_kh 
                AND @ngay_ct BETWEEN a.ngay_bd AND a.ngay_kt 
                ORDER BY a.ngay_bd DESC";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_cttc",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cttc ?? ""
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kh ?? ""
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });

            DataSet dataSet = base.ExecSql2DataSet(sql, paras, ConnectType.Accounting);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                DataRow row = dataSet.Tables[0].Rows[0];
                return new Dictionary<string, string>
                {
                    { "loai_kho_nhap", row["loai_kho_nhap"].ToString().Trim() },
                    { "ten_loai", row["ten_loai"].ToString().Trim() }
                };
            }

            return null;
        }

    }
}
