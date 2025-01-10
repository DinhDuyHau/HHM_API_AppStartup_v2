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


namespace Report.RptPaymentBank
{
    public class Service : IReportService
    {
        public IMemoryCache MemoryCache { get; set; }

        public IConfiguration Configuration { get; set; }

        public string controller { get; set; } = "rptPaymentBank";

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
            int user_id = Startup.UserId;
            int admin = Startup.Admin;
            string ma_dvcs = Startup.Unit;

            string ma_vv = "";
            string ma_hd = "";
            string ma_bp = "";
            string ma_lo = "";
            string ma_td1 = "";
            string ma_td2 = "";
            string ma_td3 = "";
            int maxLength = 12;

            sql = @" declare @voucherList varchar(512)
            select @tu_ngay as tu_ngay, @den_ngay as den_ngay
            exec rs_rptPaymentBank_Acc @tu_ngay, @den_ngay, @so_ct1, @so_ct2, @ma_kh, '', @ma_vv, @ma_hd, @ma_bp, @ma_dvcs, @ma_lo, @ma_td1, @ma_td2, @ma_td3,
								            '', '2', @maxLength, N'a.ngay_ct, a.ma_ct, a.so_ct', '1', @ma_cuahang, @ma_ca, 'v', @user_id, @admin,
								            @ck_yn, @tk_nh, @qt_yn, @ma_posqt, @qttg_yn, @ma_posqttg, @ma_khqttg, @vdt_yn, @ma_khvdt, @vnpay_yn
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
                ParameterName = "@so_ct1",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.so_ct1
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@so_ct2",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.so_ct2
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
                SqlValue = obj_param.tk
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
                ParameterName = "@maxLength",
                SqlDbType = SqlDbType.Int,
                SqlValue = maxLength
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
                ParameterName = "@ck_yn",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.ck_yn ? 1 : 0
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@tk_nh",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.tk_nh
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@qt_yn",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.qt_yn ? 1 : 0
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_posqt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_posqt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@qttg_yn",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.qttg_yn ? 1 : 0
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_posqttg",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_posqttg
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_khqttg",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_khqttg
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@vdt_yn",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.vdt_yn ? 1 : 0
            }); 
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@ma_khvdt",
                SqlDbType = SqlDbType.VarChar,
                SqlValue = obj_param.ma_khvdt
            });
            list_paras.Add(new SqlParameter
            {
                ParameterName = "@vnpay_yn",
                SqlDbType = SqlDbType.Int,
                SqlValue = obj_param.vnpay_yn ? 1 : 0
            });
            return list_paras;
        }

    }
}
