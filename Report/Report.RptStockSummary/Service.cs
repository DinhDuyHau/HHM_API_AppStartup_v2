using Genbyte.Component.Report;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace Report.RptStockSummary
{
    public class Service : IComponentService
    {
        public IMemoryCache MemoryCache { get; set; }
        public IConfiguration Configuration { get; set; }
        public string controller { get; set; } = "rptStockSummary";

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, 1);
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
            sql = @"declare @c varchar(1024)
                    select @c = case @nh_theo when '0' then 'loai_vt' when '1' then 'nh_vt1' else '' end
                    if (@c = 'nh_vt1') and (cast(@tt_sx1 as tinyint) + cast(@tt_sx2 as tinyint) + cast(@tt_sx3 as tinyint) <> 0) begin
                      select @c = @tt_sx1 + ',' + @tt_sx2 + ',' + @tt_sx3
                      select @c = replace(replace(replace(@c, '1', 'nh_vt1'), '2', 'nh_vt2'), '3', 'nh_vt3')
                      select @c = replace(replace(replace(@c, '0,', ''), ',0', ''), '0', '')
                    end

                    select cast(@tu_ngay as smalldatetime) as date_from, cast(@den_ngay as smalldatetime) as date_to, cast(@in_sl as tinyint) as in_sl,
                          convert(varchar, @tu_ngay, 103) as tu_ngay, convert(varchar, @den_ngay, 103) as den_ngay
                    exec rs_rptStockSummary @tu_ngay, @den_ngay, @ma_cuahang, @ma_kho, @ma_vt, @ma_dvcs, @loai_vt, @nh_vt1, @nh_vt2, @nh_vt3, @tinh_ps, @c, 'ma_vt', @in_theo, @loai_du_lieu, @language, @userID, @admin
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
                ParameterName = "@ma_kho",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_kho
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_cuahang == null ? Startup.Shop : obj_param.ma_cuahang
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vt",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_vt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dvcs",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_dvcs
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loai_vt",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.loai_vt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt1",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.nh_vt1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt2",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.nh_vt2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt3",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.nh_vt3
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tt_sx1",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.tt_sx1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tt_sx2",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.tt_sx2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tt_sx3",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.tt_sx3
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tinh_ps",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.tinh_ps
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_theo",
                SqlDbType = SqlDbType.NVarChar,
                SqlValue = obj_param.group
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@in_theo",
                SqlDbType = SqlDbType.NVarChar,
                SqlValue = obj_param.order
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loai_du_lieu",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.dataType
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
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@in_sl",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.in_sl
            });
            return list_paras;
        }
    }
}