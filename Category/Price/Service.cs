using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;

namespace Price
{
    public class Service : CoreService
    {
        public ServicePriceModel GetPriceOfServiceItem(string ma_dichvu, string ma_cuahang, decimal gia_ban) 
        { 
            string sql = "exec Genbyte$Service$GetPrice @service_id, @shop_id, @price";
            List<SqlParameter> paras = new List<SqlParameter>();
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
    }
}
