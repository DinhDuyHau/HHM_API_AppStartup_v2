using Genbyte.Component.Report;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace Report.RptSalespersonCommission
{
    public class Service : IComponentService
    {
        public IConfiguration Configuration { get; set; }
        public IMemoryCache MemoryCache { get; set; }
        public string controller { get; set; } = "rptSalespersonCommission";
        // Bảng hiển thị lên báo cáo.
        public readonly int id = 1;

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, id);
            return raw_model;
        }

        public CommonObjectModel GetPdfReport(string sysid, string service_url, string controllerReport, string form_id, Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetPdfReport(sysid, service_url, controller, controllerReport, form_id, sql, list_paras);
            return raw_model;
        }

        public List<SqlParameter> init(ParamItem obj_param, out string sql)
        {
            sql = @"
                declare @CustomerName nvarchar(1024)
                select @CustomerName = ten_kh from dmkh where ma_kh = @ma_kh
                select cast(@tu_ngay as smalldatetime) as date_from, cast(@den_ngay as smalldatetime) as date_to, @ma_kh ma_kh, @CustomerName as ten_kh
                exec rs_rptSalespersonCommission @tu_ngay, @den_ngay, @ma_kh, @language, @userID, @admin
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
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_kh == null ? Startup.Shop : obj_param.ma_kh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@language",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.language
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@userID",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.userId
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@admin",
                SqlDbType = SqlDbType.Bit,
                SqlValue = obj_param.admin
            });
            return list_paras;
        }
    }
}