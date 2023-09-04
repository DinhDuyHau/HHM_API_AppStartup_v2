using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customer.Model;

namespace Customer
{
    public class Service : CoreService
    {
        public List<PaymentDebtModel> GetPaymentDebit(string ma_kh, string ma_dvcs, DateTime ngay_ct)
        {
            string sql = "exec Genbyte$Customer$GetPaymentDebit @ma_kh, @ma_dvcs, @ngay_ct";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kh
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_dvcs",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_dvcs
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            List<PaymentDebtModel> entities = base.ExecSql2List<PaymentDebtModel>(sql, paras, ConnectType.Report);
            return entities;
        }
    }
}
