using Genbyte.Component.Report;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace Report.RptEndofShiftReport
{
    public class Service : IComponentService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptEndofShiftReport";

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";

            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, 6);
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

        public List<SqlParameter> init(ParamItem obj_param, out string sql)
        {
            // lấy cửa hàng mặc định đăng nhập
            string ma_cuahang = Startup.Shop;
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            
            sql = @"declare @StoreName nvarchar(250), @ShiftName nvarchar(250)
select @StoreName = ten_cuahang from dmcuahang where ma_cuahang = @ma_cuahang
select @ShiftName = ten_ca from dmca where ma_ca = @ma_ca
select cast(@tu_ngay as smalldatetime) as date_to, @ma_cuahang as ma_cuahang, @ma_ca as ma_ca, @StoreName as ten_cuahang, @ShiftName as ten_ca
exec rs_rptEndofShiftReport @tu_ngay, @ma_cuahang, @ma_ca, 'v', @user_id, @admin
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
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_cuahang
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_ca",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_ca
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
                SqlDbType = SqlDbType.Int,
                SqlValue = admin
            });

            return list_paras;
        }

    }
}
