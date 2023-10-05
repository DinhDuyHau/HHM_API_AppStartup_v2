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
        public DiscountCodeModel GetDiscountCodeInfo(string ma_gg, List<string> list_vt)
        {
            string str_vt = string.Join(", ", list_vt);

            string sql = 
                @"select ma_gg, ma_ctr, tien_gg_nt into #temp_gg from dmmagg where ma_gg = @ma_gg and active_yn = 0 and status = '1' and ngay_het_han >= CONVERT(DATE, GETDATE())
select top 1 a.* from #temp_gg a join dmtd1 b on a.ma_ctr = b.ma_td where nh_vt in (select nh_vt2 from dmvt where dbo.ff_ExactInlist(ma_vt, @str_vt) = 1)
drop table #temp_gg";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_gg",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_gg
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@str_vt",
                SqlDbType = SqlDbType.VarChar,
                Value = str_vt
            });
            DiscountCodeModel entities = base.ExecSql2List<DiscountCodeModel>(sql, paras, ConnectType.Accounting).FirstOrDefault();
            return entities;
        }
    }
}
