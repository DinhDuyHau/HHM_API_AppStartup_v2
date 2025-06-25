using Genbyte.Base.CoreLib;
using Genbyte.Component.Report;
using Genbyte.Component.Report.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;


namespace Report.RptCrossSellingRate
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }
        public string controller { get; set; } = "rptCrossSellingRate";

        private ConnectType connectType = ConnectType.Report;

        // Bảng hiển thị lên báo cáo.
        public readonly int table_index = 1;

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, table_index, connectType);
            return raw_model;
        }

        public CommonObjectModel GetPdfReport(string sysid, string service_url, string controllerReport, string form_id, Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetPdfReport(sysid, service_url, controller, controllerReport, form_id, sql, list_paras, connectType);
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
            int user_id = Startup.UserId;
            int admin = Startup.Admin;

            sql = @"select cast(@tu_ngay as smalldatetime) as date_from, cast(@den_ngay as smalldatetime) as date_to
                    exec rs_rptCrossSellingRate @tu_ngay, @den_ngay, @ma_nvbh, @ma_asm, @ma_cuahang, @nh_vt1, @nh_vt2, @nh_vt3, @ma_nganh, 
							@nh_vt1_bk, @nh_vt2_bk, @nh_vt3_bk, @ma_nganh_bk, @mau_bc, @loginShop, 'v', @user_id, @admin";
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
                ParameterName = "@ma_nvbh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nvbh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_asm",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_asm
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_cuahang
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@mau_bc",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.mau_bc
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt3",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt3
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nganh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nganh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt1_bk",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt1_bk
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt2_bk",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt2_bk
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt3_bk",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt3_bk
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nganh_bk",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nganh_bk
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
                ParameterName = "@loginShop",
                SqlDbType = SqlDbType.Char,
                SqlValue = Startup.Shop
            });
            return list_paras;
        }
    }
}