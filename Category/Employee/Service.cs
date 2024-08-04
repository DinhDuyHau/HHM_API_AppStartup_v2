using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Employee.Model;
using Microsoft.AspNetCore.Mvc;
using Genbyte.Sys.Common;

namespace Employee
{
    public class Service : CoreService
    {
        public EntityCollection<EmployeeModel> GetListEmployee(string ma_nvbh, string? order_by, int page_index = 1, int page_size = 0)
        {
            EntityCollection<EmployeeModel> entity = null;
            string sql = "select top 1 rtrim(id) as id,name,rtrim(comment) as comment,rtrim(user_lst) as user_lst from userinfo2 where userinfo2.id in (select id from userinfo2 where user_yn = 0) and name = 'ASM' ";
            string user_lst = base.ExecSql2DataSet(sql, null, ConnectType.Sys).Tables[0].Rows[0]["user_lst"].ToString();


            string sql2 = @"select * from dmnvbh where ma_nvbh like '%' + @ma_nvbh + '%' and m_username in (select ltrim(data) as m_username from [dbo].[MokaOnline$Function$System$Split](@user_lst, ',')) ";
            if(order_by != null && order_by != "")
            {
                sql += $"order by {order_by}";
            }
            List<SqlParameter> paras = new List<SqlParameter>();

            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_nvbh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_nvbh
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_lst",
                SqlDbType = SqlDbType.VarChar,
                Value = user_lst
            });

            List<EmployeeModel> raw_data = base.ExecSql2List<EmployeeModel>(sql2, paras, ConnectType.Accounting);
            entity = DataUtils.PagingProcess<EmployeeModel>(raw_data, page_index, page_size);
            return entity;
        }

        public EntityCollection<UserModel> GetListUser(string name, string type, string? order_by, int page_index = 1, int page_size = 0)
        {
            EntityCollection<UserModel> entity = null;
            string sql = @"declare @user_lst nvarchar(4000)
                           select top 1 @user_lst = rtrim(user_lst) from userinfo2 where userinfo2.id in (select id from userinfo2 where user_yn = 0) and name = @type
                           select name, comment, comment2 from userinfo2 where dbo.ff_ExactInlist(name, @user_lst) = 1";
            if (order_by != null && order_by != "")
            {
                sql += $"order by {order_by}";
            }

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@type",
                SqlDbType = SqlDbType.VarChar,
                Value = type
            });

            List<UserModel> raw_data = base.ExecSql2List<UserModel>(sql, paras, ConnectType.Sys);
            if(!string.IsNullOrEmpty(name))
            {
                raw_data = raw_data.Where(x=>x.name.Contains(name)).ToList();
            }
            entity = DataUtils.PagingProcess<UserModel>(raw_data, page_index, page_size);
            return entity;
        }
        public bool checkUser(string name, string type)
        {
            string sql = @"select top 1 name, comment, comment2 from userinfo2 where userinfo2.id in (select id from userinfo2 where user_yn = 0) and name = @type and dbo.ff_ExactInlist(@name, rtrim(user_lst)) = 1";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@type",
                SqlDbType = SqlDbType.VarChar,
                Value = type
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@name",
                SqlDbType = SqlDbType.VarChar,
                Value = name
            });
            List<UserModel> raw_data = base.ExecSql2List<UserModel>(sql, paras, ConnectType.Sys);
            if(raw_data != null && raw_data.Count == 1)
            {
                return true;
            }
            return false;
        }

        public EmployeeModel GetEmployee(string name, string ma_cuahang = "")
        {
            string sql = @"select top 1 * from dmnvbh where m_username = @name order by datetime2 desc";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@name",
                SqlDbType = SqlDbType.VarChar,
                Value = name
            });

            List<EmployeeModel> raw_data = base.ExecSql2List<EmployeeModel>(sql, paras, ConnectType.Accounting);
            if(raw_data.Count == 1)
            {
                return raw_data[0];
            }
            return null;
        }

        public DepartmentModel GetDepartment(string ma_kh, string ma_cuahang)
        {
            string sql = @"select top 1 * from dmnvls a left join dmbp b on a.ma_bp = b.ma_bp where ma_nvbh = @ma_kh and ngay_bd <= GETDATE() and (a.ma_cuahang is null or a.ma_cuahang = '' or a.ma_cuahang = @ma_cuahang) order by ngay_bd desc, a.ma_cuahang desc, a.datetime2 desc";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kh
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang
            });

            List<DepartmentModel> raw_data = base.ExecSql2List<DepartmentModel>(sql, paras, ConnectType.Accounting);
            if (raw_data.Count == 1)
            {
                return raw_data[0];
            }
            return null;
        }
    }
}
