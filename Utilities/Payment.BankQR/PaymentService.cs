using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;

namespace Payment.BankQR
{
    public class PaymentService : CoreService
    {
        /// <summary>
        /// Lấy id tương ứng của đối tác theo mã đối tác
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetParnerIdByName(IMemoryCache memory_cache, string name)
        {
            string parner_id = "";
            string key = $"QrPayment$AuthCode${name.ToUpper()}";

            // Kiểm tra auth_code trong memory cache => nếu không tồn tại sẽ lấy từ db lưu vào cache
            if (!memory_cache.TryGetValue<string>(key, out parner_id))
            {
                string sql = "select * from parner_userinfo where name = @name";
                List<SqlParameter> paras = new List<SqlParameter>();
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@name",
                    SqlDbType = SqlDbType.VarChar,
                    Value = name.Trim().Replace("'", "''")
                });
                DataSet ds = base.ExecSql2DataSet(sql, paras, ConnectType.Sys);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    parner_id = ds.Tables[0].Rows[0]["parner_id"].ToString().Trim();
                }

                MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    AbsoluteExpiration = DateTime.Now.AddMinutes(60)
                };
                memory_cache.Set(key, parner_id, cacheOptions);
            }

            return parner_id; 
        }
    }
}
