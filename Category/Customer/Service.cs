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
        public List<PaymentDepositModel> GetPaymentDeposit(string ma_kh, string ma_dvcs, string ma_ctr, string ten_ctr, string ma_vt, string ten_vt, DateTime ngay_ct)
        {
            string sql = "exec Genbyte$Customer$GetPaymentDeposit @ma_kh, @ma_dvcs, @ma_ctr, @ten_ctr, @ma_vt, @ten_vt, @ngay_ct";
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
                ParameterName = $"@ma_ctr",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_ctr
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ten_ctr",
                SqlDbType = SqlDbType.VarChar,
                Value = ten_ctr
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_vt",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ten_vt",
                SqlDbType = SqlDbType.VarChar,
                Value = ten_vt
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            List<PaymentDepositModel> entities = base.ExecSql2List<PaymentDepositModel>(sql, paras, ConnectType.Report);
            return entities;
        }
    }
}
