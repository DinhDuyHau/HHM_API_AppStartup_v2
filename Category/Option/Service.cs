using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Option.Model;

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
    }
}
