using Genbyte.Component.Report;
using Genbyte.Component.Report.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace Report.RptSuppliesOnTheRoadReport
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptSuppliesOnTheRoadReport";

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
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            int maxLength = 12;

            sql = @"select @tu_ngay as tu_ngay, @den_ngay as den_ngay, @ma_cuahang_x as ma_cuahang_x, @ma_cuahang_n as ma_cuahang_n,
                            @ma_kho_x as ma_kho_x, @ma_kho_n as ma_kho_n
		            exec rs_rptSuppliesOnTheRoadReport @tu_ngay, @den_ngay, @tu_so, @den_so, @ma_cuahang_x, @ma_cuahang_n, @ma_kho_x,
                        @ma_kho_n, @ma_ca, @ma_vt, '', @maxLength, 'v', @user_id, @admin, @nh_vt1, @nh_vt2, @nh_vt3, @nh_vt4,
                        @nh_vt5, @nh_vt6, @ma_nganh, @loai, @loginShop";
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
                ParameterName = "@tu_so",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.tu_so
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@den_so",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.den_so
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang_x",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_cuahang_x
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang_n",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_cuahang_n
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kho_x",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_kho_x
            });
            list_paras.Add(new SqlParameter
            {
                 ParameterName = "@ma_kho_n",
                 SqlDbType = SqlDbType.VarChar,
                 SqlValue = obj_param.ma_kho_n
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_ca",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_ca
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_vt
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
                ParameterName = "@nh_vt4",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt4
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt5",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt5
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt6",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.nh_vt6
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nganh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nganh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loai",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.loai
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@maxLength",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength
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
