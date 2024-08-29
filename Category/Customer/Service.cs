using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customer.Model;
using Newtonsoft.Json;
using System.Reflection.Metadata;

namespace Customer
{
    public class Service : CoreService
    {
        #region GetPaymentDebit
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
        #endregion

        #region GetPaymentDeposit
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
        #endregion

        #region GetConversionPoint
        public decimal GetConversionPoint(string ma_kh, DateTime ngay_ct)
        {
            string sql = "EXEC Genbyte$Customer$GetConversionPoint @ma_kh, @ngay_ct";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_kh",
                SqlDbType = SqlDbType.Char,
                Value = ma_kh
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });
            decimal conversionPoint = (decimal)base.ExecSql2DataSet(sql, paras, ConnectType.Report).Tables[0].Rows[0]["diem_qd"];
            return conversionPoint;
        }
        #endregion

        #region GetCustomerInfoByTax
        public CustomerInfoByTax GetCustomerInfoByTax(string service_url, string ma_so_thue)
        {
            var res = this.GetCustomerInfo(service_url, ma_so_thue).Result.ToString();
            if (res.Contains("error"))
            {
                return new CustomerInfoByTax()
                {
                    error = true
                };
            }
            else
            {
                var customer = JsonConvert.DeserializeObject<CustomerInfoByTax>(res);
                customer.error = false; 
                return customer;
            }
        }
        #endregion

        #region GetCustomerInfo
        private async Task<object> GetCustomerInfo(string service_url, string ma_so_thue)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    string postData = "{\"taxCode\": \"" + ma_so_thue + "\"}";
                    HttpContent httpContent = new StringContent(postData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(service_url, httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        // Đọc nội dung phản hồi
                        string content = await response.Content.ReadAsStringAsync();
                        return content;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (HttpRequestException ex)
                {
                    return null;
                }
            }
        }
        #endregion

    }
}
