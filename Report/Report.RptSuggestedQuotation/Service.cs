using Genbyte.Component.Report;
using Genbyte.Component.Report.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Report.RptSuggestedQuotation
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }
        public IConfiguration Configuration { get; set; }
        public string controller { get; set; } = "rptSuggestedQuotation";
        public readonly int table_index = 1; // Bảng hiển thị lên báo cáo.
        public readonly int table_pivot_index = 0; // Bảng tạo cột báo cáo xoay.
        public bool isPivotReport = true; // Báo cáo xoay
        public PivotReportConfig pivotReportConfig = new PivotReportConfig() // khai báo cấu hình báo cáo xoay
        {
            PivotDataField = "name",
            PivotHeaderField = "header",
            GroupByFields = new List<string> { "ma_vt", "ten_vt" },
            SumFields = new List<string> { "so_luong" },
            ExtraTableIndex = 1,
            KeyField = "sysColumn"
        };

        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            string sql;
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, table_index, ConnectType.Report, "", isPivotReport, table_pivot_index, pivotReportConfig);
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

        public Query InitExport(string controller, Dictionary<string, object> param)
        {
            string sql = "";
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            List<SqlParameter> list_paras = init(obj_param, out sql);
            return new Query()
            {
                SqlString = sql,
                Parameters = list_paras,
                RptTableIndex = 0,
                PivotReportConfig = pivotReportConfig
            };
        }

        public List<SqlParameter> init(ParamItem obj_param, out string sql)
        {
            // lấy cửa hàng mặc định đăng nhập
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            int loai_du_lieu = 2; //1: thực tế, 2: hóa đơn

            sql = $"exec rs_rptSuggestedQuotation @ngay, @ma_nh, @ma_cuahang, @ma_vt, @nh_vt1, @nh_vt2, @nh_vt3, @nh_vt4, @nh_vt5, @ma_nganh, 'v', @user_id, @admin";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.den_ngay
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nh",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_nh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_cuahang
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_vt",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_vt
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
                ParameterName = "@nh_vt4",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.nh_vt4
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@nh_vt5",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.nh_vt5
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
                ParameterName = "@ma_nganh",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_nganh
            });
            return list_paras;
        }
    }
}
