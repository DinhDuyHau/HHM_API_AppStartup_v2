using Genbyte.Component.Report;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace Report.RptImeiAgeReport
{
    public class Service : IComponentService
    {
        public IMemoryCache MemoryCache { get; set; }
        public string controller { get; set; } = "rptImeiAgeReport";
        // Bảng hiển thị lên báo cáo.
        public readonly int id = 1;

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param.page_index, obj_param.page_size, id);
            return raw_model;
        }

        public CommonObjectModel GetPdfReport(string sysid, string service_url, string controllerReport, string form_id, Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache);
            CommonObjectModel raw_model = data_utis.GetPdfReport(sysid, service_url, controller, controllerReport, form_id, sql, list_paras);
            return raw_model;
        }

        public List<SqlParameter> init(ParamItem obj_param, out string sql)
        {
            sql = @"
                select cast(@tu_ngay as smalldatetime) as date_from, cast(@den_ngay as smalldatetime) as date_to, @ma_cuahang ma_cuahang
                exec rs_rptImeiAgeReport @ma_imei, @tu_ngay, @den_ngay, @ma_cuahang, @language, @userID, @admin
            ";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_imei",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_imei
            });
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
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_cuahang == null ? Startup.Shop : obj_param.ma_cuahang
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