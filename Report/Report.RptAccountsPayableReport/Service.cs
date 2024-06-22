using Genbyte.Base.CoreLib;
using Genbyte.Component.Report;
using Genbyte.Component.Report.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;


namespace Report.RptAccountsPayableReport
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public class ParamOption
        {
            public string name { get; set; }

            public string val { get; set; } 

        }
        public string controller { get; set; } = "rptAccountsPayableReport";

        // Bảng hiển thị lên báo cáo.
        public readonly int table_index = 1;

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";

            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, table_index);
            return raw_model;
        }

        public CommonObjectModel GetPdfReport(string sysid, string service_url, string controllerReport, string form_id, Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetPdfReport(sysid, service_url, controller, controllerReport, form_id, sql, list_paras);
            return raw_model;
        }

        public Query InitExport(string controller, Dictionary<string, object> param)
        {
            string sql = "";
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            List<SqlParameter> list_paras = init(obj_param, out sql);
            return new Query()
            {
                SqlString = sql,
                Parameters = list_paras,
                RptTableIndex = this.table_index
            };
        }

        public List<SqlParameter> init(ParamItem obj_param, out string sql)
        {
            // lấy cửa hàng mặc định đăng nhập
            string ma_cuahang = Startup.Shop;
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            //string ma_nvbh = "";
            string ma_dvcs = Startup.Unit;
           
            string nh_kh1 = "";
            string nh_kh2 = "";
            string nh_kh3 = "";
            string tk = "";

            string sql2 = "select val from options where name = 'cn_ptra'";
            CoreService coreservice = new CoreService();
            //DataSet ds = coreservice.ExecSql2DataSet(sql2, paras: null, conn_type: ConnectType.Accounting);
            List<ParamOption> paramOptions = coreservice.ExecSql2List<ParamOption>(sql2, paras: null, conn_type: ConnectType.Accounting);
            if (paramOptions != null && paramOptions.Count > 0)
            {
                tk = paramOptions[0].val;
            }
            sql = @"select @tu_ngay as date_from, @den_ngay as date_to
                    exec rs_rptAccountsPayableReport @tu_ngay, @den_ngay, @tk, @ma_kh, @nh_kh1, @nh_kh2, @nh_kh3, '', 'ma_kh',
                           @ma_dvcs, 'v', @user_id, @admin
            ";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tu_ngay",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.tu_ngay
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@den_ngay",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.den_ngay
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tk",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = tk
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_kh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_kh1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = nh_kh1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_kh2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = nh_kh2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_kh3",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = nh_kh3
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dvcs",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_dvcs
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@user_id",
                SqlDbType = SqlDbType.Int,
                SqlValue = user_id
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@admin",
                SqlDbType = SqlDbType.Bit,
                SqlValue = admin
            });

            return list_paras;
        }

    }
}
