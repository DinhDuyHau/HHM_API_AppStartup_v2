using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using Report;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Report.Model;

namespace Report
{
    public class Service : CoreService
    {
       public async Task<object> getPdfVoucher(string sysid, string service_url, string ma_dvcs, string key, string controller, string form_id)
       {
            var soapClient = new SoapClient();

            // Địa chỉ URL của dịch vụ web SOAP
            string soapEndpointUrl = service_url + "?op=ExportToBase64";

            // Nội dung yêu cầu SOAP dưới dạng chuỗi XML
            string soapXmlBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
              <soap12:Body>
                <ExportToBase64 xmlns=""http://tempuri.org/"">
                  <sysid>{0}</sysid>
                  <unit>{1}</unit>
                  <key>{2}</key>
                  <controller>{3}</controller>
                  <form_id>{4}</form_id>
                </ExportToBase64>
              </soap12:Body>
            </soap12:Envelope>";
            soapXmlBody = string.Format(soapXmlBody, sysid, ma_dvcs, key, controller, form_id);

            // Gọi dịch vụ web SOAP và nhận kết quả phản hồi
            string soapResponse = await soapClient.CallSoapService(soapEndpointUrl, soapXmlBody);
            return soapResponse;
       }
        public string getPdf(string sysid, string service_url, string ma_dvcs, string key, string controller, string form_id)
        {
            string res = this.getPdfVoucher(sysid, service_url, ma_dvcs, key, controller, form_id).Result.ToString();
            var soapClient = new SoapClient();
            string pdf = soapClient.ExtractExportToBase64Result(res, "ExportToBase64Result");
            return pdf;
        }

        public List<ReportMenu> getReportMenu(string sysid)
        {
            string sql = $"select * from api_menu_report where sysid = '{sysid}' and status = '1' order by sort asc";
            DataSet ds = ExecSql2DataSet(sql);
            IList<ReportMenu> reportMenus = new List<ReportMenu>();
            if (ds != null && ds.Tables.Count > 0)
            {
                reportMenus = ds.Tables[0].ToList<ReportMenu>();
            }
            return reportMenus.ToList();
        }
        public List<ReportMenu> getReportMenu()
        {
            string sql = $"select * from api_menu_report where status = '1' order by sysid, controller, sort, form_id";
            DataSet ds = ExecSql2DataSet(sql);
            IList<ReportMenu> reportMenus = new List<ReportMenu>();
            if (ds != null && ds.Tables.Count > 0)
            {
                reportMenus = ds.Tables[0].ToList<ReportMenu>();
            }
            return reportMenus.ToList();
        }
    }
}
