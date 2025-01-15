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

namespace Report.RptIssueTransactionListByImei
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptIssueTransactionListByImei";

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
            string ma_vv = "";
            string ma_nx = "";
            string tk_vt = "";
            string ma_loai_vt = "";
            string ma_hd = "";
            string ma_bp = "";
            string lenh_sx = "";
            string ma_sp = "";
            string so_ct1 = "";
            string so_ct2 = "";
            int maxLength = 12;
            int maxLength_mo = 16;
            string ma_nh_kho = "";

            sql = @"
                declare @voucherList varchar(512)
                select @voucherList = 'PXB';
                select cast(@tu_ngay as smalldatetime) as tu_ngay, cast(@den_ngay as smalldatetime) as den_ngay
                exec rs_rptIssueTransactionListByImei @tu_ngay, @den_ngay, @loginShop, @ma_cuahang, @ma_vt, @ma_imei, @ma_kh, @ma_kho, @kho_hang_dc, @ma_vv, @ma_nx, @tk_vt, 
                  @ma_loai_vt, @nh_vt1, @nh_vt2, @nh_vt3, 3, @voucherList, @so_ct1, @so_ct2, @ma_hd, @ma_bp, @lenh_sx, 
                  @ma_sp, @ma_dvcs, @maxLength, @maxLength_mo, 2, 'a.ma_kh', @loai_du_lieu, 'v', @user_id, @admin, @ma_nh_kho, @mau_bc
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
                ParameterName = "@ma_vt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_vt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_imei",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_imei
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
                ParameterName = "@kho_hang_dc",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.kho_hang_dc
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vv",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_vv
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nx",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_nx
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tk_vt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = tk_vt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_loai_vt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_loai_vt
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
                ParameterName = "@lenh_sx",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = lenh_sx
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_sp",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_sp
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dvcs",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_dvcs
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@maxLength",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@maxLength_mo",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength_mo
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loai_du_lieu",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.loai_du_lieu
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
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_cuahang ?? ""
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loginShop",
                SqlDbType = SqlDbType.Char,
                SqlValue = Startup.Shop
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nh_kho",
                SqlDbType = SqlDbType.Char,
                SqlValue = ma_nh_kho
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@mau_bc",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.mau_bc
            });
            return list_paras;
        }

    }
}
