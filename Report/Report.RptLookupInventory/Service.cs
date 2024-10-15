using Genbyte.Base.CoreLib;
using Genbyte.Component.Report;
using Genbyte.Component.Report.Model;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;


namespace Report.RptLookupInventory
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptLookupInventory";

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
            // lấy mặc định đăng nhập
            int user_id = Startup.UserId;
            int admin = Startup.Admin;

        sql = @"declare @c varchar(1024)
            select @c = case @nh_theo when '0' then '' when '1' then 'loai_vt' when '2' then 'nh_vt1' else '' end
            if (@c = 'nh_vt1') and (cast(@tt_sx1 as tinyint) + cast(@tt_sx2 as tinyint) + cast(@tt_sx3 as tinyint) <> 0) begin
              set @c = @tt_sx1 + ',' + @tt_sx2 + ',' + @tt_sx3
              set @c = replace(replace(replace(@c, '1', 'nh_vt1'), '2', 'nh_vt2'), '3', 'nh_vt3')
              set @c = replace(replace(replace(@c, '0,', ''), ',0', ''), '0', '')
            end

            declare @SiteName nvarchar(1024), @SiteName2 nvarchar(1024)
            if exists (select ma_kho from dmkho where ma_kho = @ma_kho)	
	              select @SiteName = ten_kho, @SiteName2 = ten_kho2 from dmkho where ma_kho = @ma_kho
            else if (@ma_kho = '')	
                select @SiteName = N'Tất cả các kho', @SiteName2 = N'All Sites'
            else 
                select @SiteName = N'', @SiteName2 = N''

            declare @tench nvarchar(1024)
            if exists(select ma_cuahang from dmcuahang where ma_cuahang = @ma_cuahang)
            select @tench = ten_cuahang from dmcuahang where ma_cuahang = @ma_cuahang
            select cast(@ngay as smalldatetime) as date_to, @ma_kho as ma_kho, @SiteName as ten_kho, @SiteName2 as ten_kho2, @ma_cuahang as ma_cuahang, @tench as ten_cuahang
            exec rs_rptLookupInventory @ngay, @loginShop, @ma_cuahang, @ma_kho, @ma_vt, @nh_vt1, @nh_vt2, @nh_vt3, @nh_vt4, @c, @ma_dvcs, @loai_ky, 'ma_vt',  @loai_du_lieu, 'v', @user_id, @admin, @nh_vt5, @nh_vt6, @ma_nganh, @ma_loai
";
            List<SqlParameter> list_paras = new List<SqlParameter>();
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ngay",
                SqlDbType = SqlDbType.DateTime,
                SqlValue = obj_param.den_ngay
            });            
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_cuahang 
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_kho",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_kho
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
                ParameterName = "@loai_ky",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.loai_ky
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
                ParameterName = "@nh_vt6",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.nh_vt6
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_nganh",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_nganh
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
                ParameterName = "@loai_du_lieu",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.dataType
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
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_loai",
                SqlDbType = SqlDbType.Char,
                SqlValue = obj_param.ma_loai
            });
            return list_paras;
        }

    }
}
