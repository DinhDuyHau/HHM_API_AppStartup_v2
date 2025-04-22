using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Option.Model;
using Genbyte.Sys.Common;

namespace Option
{
    public class Service : CoreService
    {
        public OptionModel GetOptionByName(string name)
        {
            string sql = "select top 1 * from options where name = @name";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@name",
                SqlDbType = SqlDbType.Char,
                Value = name
            });
            return base.ExecSql2List<OptionModel>(sql, paras, ConnectType.App)[0];
        }
        public List<OptionModel> GetOptionBySubSystem(string ma_phan_he)
        {
            string sql = "select * from options where ma_phan_he = @ma_phan_he";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_phan_he",
                SqlDbType = SqlDbType.Char,
                Value = ma_phan_he
            });
            return base.ExecSql2List<OptionModel>(sql, paras, ConnectType.App);
        }

        /** Lấy danh sách voucher */
        public List<DiscountVoucherModel> GetDiscountVoucherCode(DateTime ngay_ct) 
        {
            DataService service = new DataService();

            string sql = @"
                SELECT TOP 1 a.*, b.ten_loai
                FROM dmck2 a
                LEFT JOIN dmloaick b ON b.ma_loai = a.loai_ck
                WHERE a.loai_ck = 10
                  AND @ngay_ct >= a.ngay_bd
                  AND @ngay_ct <= a.ngay_kt
                ORDER BY a.ngay_bd DESC";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ngay_ct",
                SqlDbType = SqlDbType.DateTime,
                Value = ngay_ct
            });

            return service.ExecSql2List<DiscountVoucherModel>(sql, paras, ConnectType.Accounting);
        }
    }
}
