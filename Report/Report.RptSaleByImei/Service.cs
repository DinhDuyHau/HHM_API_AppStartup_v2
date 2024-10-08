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


namespace Report.RptSaleByImei
{
    public class Service : IComponentService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptSaleByImei";

        private const int M_TABLE_VIEW_INDEX = 1;

        private ConnectType connectType = ConnectType.Report;
        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);        
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, M_TABLE_VIEW_INDEX, connectType);
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

        public List<SqlParameter> init(ParamItem obj_param, out string sql)
        {
            // lấy cửa hàng mặc định đăng nhập
           
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            string ma_dvcs = "";

            string so_ct1 = "";
            string so_ct2 = "";
            string ma_imei = "";
            string tk_dt = "";
            string tk_vt = "";
            string ma_nx = "";
            string ma_vv = "";
            string ma_hd = "";
            string ma_bp = "";
            string ma_lo = "";
            string ma_td1 = "";
            string ma_td2 = "";
            string ma_td3 = "";
            string ds_ma_gd = "";
            int maxLength = 12;
            int loai_du_lieu = 1;
            string ma_nh_kho = "";

            sql = @"declare @voucherList varchar(512)
                    select @voucherList = 'HDA, E01, DX1, DXA'
                    select @ngay_ct1 as tu_ngay, @ngay_ct2 as den_ngay
                    exec rs_rptSaleByImei @ngay_ct1, @ngay_ct2, @so_ct1, @so_ct2, @ma_nvbh, @ma_kh, @ma_kho, @ma_vt, @ma_dv, @ma_imei, @tk_dt, @tk_vt,
                        @nh_vt1, @nh_vt2, @nh_vt3, @ma_nx, @ma_vv, @ma_hd, @ma_bp, @ma_dvcs, @ma_lo, @ma_td1, @ma_td2, @ma_td3, @ds_ma_gd, @voucherList, 
                        '2', @maxLength, N'a.ngay_ct, a.ma_ct, a.so_ct', @loai_du_lieu, @ma_cuahang, @ma_ca, @nh_vt4, @nh_vt5, @ma_nganh, 'v', @user_id, @admin, @loginShop, @ma_nh_kho";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_ct1",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.tu_ngay
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay_ct2",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.den_ngay
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
                ParameterName = "@ma_nvbh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nvbh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_kh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kho",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_kho
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_vt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dv",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_dv
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_imei",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_imei
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tk_dt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = tk_dt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tk_vt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = tk_vt
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
                ParameterName = "@ma_nx",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_nx
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vv",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_vv
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_hd",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_hd
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_bp",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_bp
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dvcs",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_dvcs
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_lo",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_lo
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_td1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_td1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_td2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_td2
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_td3",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_td3
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ds_ma_gd",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ds_ma_gd
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@maxLength",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loai_du_lieu",
                SqlDbType = SqlDbType.Int,
                SqlValue = loai_du_lieu
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
                ParameterName = "@ma_nganh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nganh
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
                ParameterName = "@ma_nh_kho",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_nh_kho
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
