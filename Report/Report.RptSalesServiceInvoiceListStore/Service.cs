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


namespace Report.RptSalesServiceInvoiceListStore
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptSalesServiceInvoiceListStore";

        public readonly int table_index = 1;

        private ConnectType connectType = ConnectType.Report;
        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);        
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, table_index, connectType);
            return raw_model;
        }

        public CommonObjectModel GetPdfReport(string sysid, string service_url, string controllerReport, string form_id, Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";
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
            string ma_dvcs = Startup.Unit;
            string tk = "";
            string ma_ct = "";
            string ma_vv = "";
            string ma_bp = "";
            string ma_hd = "";
            string voucher_from = "";
            string voucher_to = "";
            int maxLength = 12;

            sql = @"
                select @date_from as tu_ngay, @date_to as den_ngay
                exec rs_rptSalesServiceInvoiceListStore @date_from, @date_to, '', @ma_kh, @tk, 'HDA, E01, HD1, HD2' , @ma_ct, @ma_vv,
                    @ma_bp, @ma_hd, @voucher_from, @voucher_to, @maxLength, @ma_dvcs, 'v', @user_id, @admin, @loginShop, @ma_cuahang, @ma_ca, @ma_dv
            ";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@date_from",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.tu_ngay
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@date_to",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.den_ngay
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_kh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tk",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = tk
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_ct",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_ct
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vv",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_vv
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_bp",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_bp
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_hd",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_hd
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@voucher_from",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = voucher_from
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@voucher_to",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = voucher_to
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@maxLength",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength
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
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_cuahang
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_ca",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_ca
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dv",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_dv
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
