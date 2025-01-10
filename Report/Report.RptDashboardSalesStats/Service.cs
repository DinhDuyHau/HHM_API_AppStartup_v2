using Genbyte.Component.Report;
using Genbyte.Component.Report.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Report.RptDashboardSalesStats
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptDashboardSalesStats";

        public readonly int table_index = 0;

        private ConnectType connectType = ConnectType.Report;
        public CommonObjectModel Execute(Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            obj_param.reset = true;
            string sql = "";
            List<SqlParameter> list_paras = init(obj_param, out sql);
            DataUtils data_utis = new DataUtils(MemoryCache, Configuration);
            CommonObjectModel raw_model = data_utis.GetDataPaging(this.controller, sql, list_paras, obj_param, table_index, connectType);
            return raw_model;
        }

        public CommonObjectModel GetPdfReport(string sysid, string service_url, string controllerReport, string form_id, Dictionary<string, object> param)
        {
            ParamItem obj_param = Converter.DictionaryToObject<ParamItem>(param);
            obj_param.reset = true;
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
            string ma_cuahang = Startup.Shop;

            sql = @"exec rs_rptDashboardSalesStats @userID, @loginShop";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@userID",
                SqlDbType = SqlDbType.Int,
                SqlValue = user_id
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@loginShop",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = ma_cuahang
            });
            return list_paras;
        }

    }
}
