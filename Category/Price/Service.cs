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
    }
}
