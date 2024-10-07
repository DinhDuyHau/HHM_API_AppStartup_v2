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


namespace Report.RptDetailDebtReport
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptDetailDebtReport";

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
            // lấy mặc định đăng nhập
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            string ma_dvcs = Startup.Unit;

            string so_ct1 = "";
            string so_ct2 = "";
            int in_hd = 1;
            int so_du = 1;          
            int maxLength = 12;


            sql = @"select cast(@ngay_ht1 as smalldatetime) as tu_ngay, cast(@ngay_ht2 as smalldatetime) as den_ngay
exec rs_rptDetailDebtReport @ngay_ht1, @ngay_ht2, @ngay_tt, @ma_dvcs, @ma_kh, @ma_nh1, @ma_nh2, @ma_nh3, @so_ct1, @so_ct2,
                            @in_hd, @maxLength, @so_du, @ngay_ct1, @ngay_ct2, 'v', @user_id, @admin, @ma_cuahang, @loginShop
";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_ht1",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.ngay_ht1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_ht2",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.ngay_ht2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_tt",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.ngay_tt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dvcs",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_dvcs
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_kh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nh1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nh1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nh2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nh2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nh3",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nh3
            });

            list_paras.Add(new SqlParameter
            {
                ParameterName = "@so_ct1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = so_ct1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@so_ct2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = so_ct2
            });
           
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@in_hd",
                SqlDbType = SqlDbType.Int,
                SqlValue = in_hd
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@maxLength",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@so_du",
                SqlDbType = SqlDbType.Int,
                SqlValue = so_du
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_ct1",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.ngay_ct1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_ct2",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.ngay_ct2
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
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_cuahang
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loginShop",
                SqlDbType = SqlDbType.Char,
                SqlValue = Startup.Shop
            });

            return list_paras;
        }

    }
}
