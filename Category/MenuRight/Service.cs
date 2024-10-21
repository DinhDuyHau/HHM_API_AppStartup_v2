using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using MenuRight.Model;
using System.Data.SqlClient;
using System.Data;

namespace MenuRight
{
    public class Service : CoreService
    {
        public List<MenuModal> GetMenuRight()
        {
            string username = Startup.UserName;
            int admin = Startup.Admin;

            string sql = "exec rs_GetMenuRights @username, @admin";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@username",
                SqlDbType = SqlDbType.VarChar,
                Value = username
            });
            paras.Add(new SqlParameter
            {
                ParameterName = "@admin",
                SqlDbType = SqlDbType.Bit,
                SqlValue = admin
            });
            List<MenuModal> menus = base.ExecSql2List<MenuModal>(sql, paras, ConnectType.Accounting);
            return menus;
        }
    }
}
