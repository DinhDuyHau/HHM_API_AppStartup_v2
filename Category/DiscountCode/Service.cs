using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscountCode.Model;
using Newtonsoft.Json;
using System.Reflection.Metadata;

namespace DiscountCode
{
    public class Service : CoreService
    {
        public DiscountCodeModel GetDiscountCodeInfo(string ma_gg)
        {
            string sql = 
                @"select top 1 ma_gg, tien_gg_nt from dmmagg where ma_gg = @ma_gg and active_yn = 0 and status = '1' and ngay_het_han >= CONVERT(DATE, GETDATE())";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_gg",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_gg
            });
            DiscountCodeModel entities = base.ExecSql2List<DiscountCodeModel>(sql, paras, ConnectType.Accounting).FirstOrDefault();
            return entities;
        }
        public List<DiscountProgramModel> GetDiscountProgram(List<string> list_vt)
        {
            string str_vt = string.Join(", ", list_vt);
            string sql =
                @"SELECT a.ma_ctr, a.ten_ctr, ngay_hl, ngay_hl2, b.ma_vt, c.ten_vt, b.tien_giam, b.tl_giam FROM phctrgiamgia a LEFT JOIN ctctrgiamgia b ON a.ma_ctr = b.ma_ctr 
                LEFT JOIN dmvt c ON b.ma_vt = c.ma_vt
                WHERE a.status = '1' and a.ngay_hl <= CONVERT(DATE, GETDATE()) AND a.ngay_hl2 >= CONVERT(DATE, GETDATE())
                AND dbo.ff_ExactInlist(b.ma_vt, @str_vt) = 1";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@str_vt",
                SqlDbType = SqlDbType.VarChar,
                Value = str_vt
            });
            List<DiscountProgramModel> entities = base.ExecSql2List<DiscountProgramModel>(sql, paras, ConnectType.Accounting).ToList();
            return entities;
        }
    }
}
