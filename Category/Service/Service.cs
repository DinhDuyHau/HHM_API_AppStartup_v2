using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Service.Model;

namespace Servive
{
    public class Service : CoreService
    {
        public List<KeyServiceModel> GetKeyService(string ma_dichvu, decimal so_luong) 
        { 
            string sql = $"select top {so_luong} a.*, b.ten_dv from dmkey a left join dmdichvu b on a.ma_dv = b.ma_dv  where a.ma_dv = @ma_dichvu and active_yn = 0 and ngay_het_han >= getdate() order by ngay_het_han asc";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@ma_dichvu",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_dichvu
            });
            List<KeyServiceModel> entities = base.ExecSql2List<KeyServiceModel>(sql, paras, ConnectType.Accounting);
            return entities;
        }

        public List<KeyToSendEmail> GetKeyToSendEmail(string stt_rec)
        {
            string sql = $"select a.*, b.ten_dv from dmkey a left join dmdichvu b on a.ma_dv = b.ma_dv where stt_rec = @stt_rec";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@stt_rec",
                SqlDbType = SqlDbType.VarChar,
                Value = stt_rec
            });
            List<KeyToSendEmail> entities = base.ExecSql2List<KeyToSendEmail>(sql, paras, ConnectType.Accounting);
            return entities;
        }
        public string GenerateHTML(List<KeyToSendEmail> serviceList)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            // Bắt đầu thẻ HTML
            htmlBuilder.Append("<!DOCTYPE html>");
            htmlBuilder.Append("<html>");
            htmlBuilder.Append("<head>");
            htmlBuilder.Append("<title>Thông tin dịch vụ: </title>");
            htmlBuilder.Append("<style>");
            // Thêm CSS tùy chỉnh
            htmlBuilder.Append(@"
            body {
                font-family: Arial, sans-serif;
                background-color: #f7f7f7;
            }
            h1 {
                color: #333;
                text-align: center;
                background-color: #005447;
                color: white;
                padding: 20px;
                margin: 0;
            }
            table {
                width: 100%;
                border-collapse: collapse;
                margin-top: 20px;
            }
            table, th, td {
                border: 1px solid #ddd;
            }
            th, td {
                padding: 10px;
                text-align: left;
            }
            th {
                background-color: #005447;
                color: white;
            }
            p {
                margin: 0;
                padding: 5px;
            }
            p:nth-child(even) {
                background-color: #f2f2f2;
            }
        ");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("</head>");
            htmlBuilder.Append("<body>");
            htmlBuilder.Append(@"
                <div
                      style=""text-align: center; padding: 16px 0px; background-color: #005447""
                    >
                      <img
                        width=""300px""
                        src=""https://hhm-shop-inv.genbyte.net/assets/images/logo.png""
                        alt=""logo""
                      />
                    </div>");
            htmlBuilder.Append("<h1 style=\"margin-top: 20px\">Thông tin dịch vụ</h1>");

            // Duyệt qua danh sách dịch vụ và tạo HTML cho từng mục
            htmlBuilder.Append("<table>");
            htmlBuilder.Append("<thead>");
            htmlBuilder.Append("<tr>");
            htmlBuilder.Append("<th style=\"text-align: center\">Tên dịch vụ</th>");
            htmlBuilder.Append("<th style=\"text-align: center\">Mã key</th>");
            htmlBuilder.Append("<th style=\"text-align: center\">Ngày hết hạn</th>");
            htmlBuilder.Append("</tr>");
            htmlBuilder.Append("</thead>");
            htmlBuilder.Append("<tbody>");

            foreach (var service in serviceList)
            {
                htmlBuilder.Append("<tr>");
                htmlBuilder.Append("<td>" + service.ten_dv + "</td>");
                htmlBuilder.Append("<td>" + service.ma_key + "</td>");
                htmlBuilder.Append("<td>" + service.ngay_het_han.ToString("dd/MM/yyyy") + "</td>");
                htmlBuilder.Append("</tr>");
            }

            htmlBuilder.Append("</tbody>");
            htmlBuilder.Append("</table>");

            htmlBuilder.Append("<p style=\"text-align: center;\">Xin vui lòng lưu trữ thông tin này một cách an toàn.</p>");

            // Kết thúc thẻ HTML
            htmlBuilder.Append("</body>");
            htmlBuilder.Append("</html>");

            return htmlBuilder.ToString();
        }

    }
}
