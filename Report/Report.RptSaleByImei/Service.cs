using Genbyte.Component.Report;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace Report.RptSaleByImei
{
    public class Service : IComponentService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptPaymentMethods";

        private const int M_TABLE_VIEW_INDEX = 1;

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql = "";

            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, M_TABLE_VIEW_INDEX);
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
            string ma_dvcs = Startup.Unit;

            sql = @"select @tu_ngay as tu_ngay, @den_ngay as den_ngay
exec rs_rptSaleByImei @tu_ngay, @den_ngay, '', '', @ma_nvbh, '', @ma_kho, @ma_vt, '', '', '',
  @nh_vt1, @nh_vt2, @nh_vt3, '', '', '', '', @ma_dvcs, '', '', '', '', '', 'HDA, E01, DX1, DXA', '1',
  '2', 12, N'a.ngay_ct, a.ma_ct, a.so_ct', 1, @ma_cuahang, @ma_ca, @nh_vt4, @nh_vt5, @nh_vt6, 'v', @user_id, @admin
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
                ParameterName = "@ma_nvbh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_nvbh
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
                ParameterName = "@nh_vt1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ""
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ""
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt3",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ""
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt4",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ""
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt5",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ""
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt6",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ""
            });


            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_dvcs",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_dvcs
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
                SqlDbType = SqlDbType.Bit,
                SqlValue = admin
            });

            return list_paras;
        }

    }
}
