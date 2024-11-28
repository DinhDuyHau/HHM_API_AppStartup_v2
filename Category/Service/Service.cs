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
using Service.ModelDV1;

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

        public object GetSoldServiceOrder(string so_ct, string ma_cuahang)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            string query = @"exec Genbyte$Service$GetSoldInvoice @so_ct, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
            new SqlParameter()
            {
                ParameterName = "@so_ct",
                SqlDbType = SqlDbType.VarChar,
                Value = so_ct.Trim()
            },new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang.Trim()
            }});
            var dataSet = this.ExecSql2DataSet(query, paras);
            object result = new object { };
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                result = new
                {
                    prime = dataSet.Tables[0].ToList<VoucherItemDV1>(),
                    detail = dataSet.Tables[1].ToList<SVDetailDV1>(),
                    thanhtoan = dataSet.Tables[2].ToList<TTDetailDV1>()
                };
            }
            return result;
        }
        public object GetSoldServiceOrders(string ma_kh, string ma_cuahang)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            string query = @"exec Genbyte$Service$GetListSoldInvoice @ma_kh, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
            new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kh.Trim()
            },new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang.Trim()
            }});
            var dataSet = this.ExecSql2DataSet(query, paras);
            object result = new object { };
            if (dataSet != null && dataSet.Tables.Count > 1)
            {
                var prime = dataSet.Tables[0].ToList<VoucherItemDV1>();
                var detail = dataSet.Tables[1].ToList<SVDetailDV1>();

                prime.ToList().ForEach(item =>
                {
                    item.items = detail.Where(x=>x.stt_rec == item.stt_rec).ToList();
                });
                return prime;
            }
            return result;
        }

        public object GetOrdersServiceReturn(string ma_kh, string ma_cuahang)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            string query = @"exec Genbyte$Service$GetListOrderServiceReturn @ma_kh, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
            new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kh
            },new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang.Trim()
            }});
            var dataSet = this.ExecSql2DataSet(query, paras);
            object result = new object { };
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                var detail = dataSet.Tables[0].ToList<SVDetailDV1>();
                return detail;
            }
            return result;
        }

        public object GetOrdersBuyBackService(string ma_kh, string ma_cuahang)
        {
            //Có thể thực hiện xử lý dữ liệu đã lấy từ db tại backend trước khi trả về client
            string query = @"exec Genbyte$Service$GetListOrderBuyBackService @ma_kh, @ma_cuahang";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.AddRange(new List<SqlParameter>() {
            new SqlParameter()
            {
                ParameterName = "@ma_kh",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_kh
            },new SqlParameter()
            {
                ParameterName = "@ma_cuahang",
                SqlDbType = SqlDbType.VarChar,
                Value = ma_cuahang.Trim()
            }});
            var dataSet = this.ExecSql2DataSet(query, paras);
            object result = new object { };
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                var detail = dataSet.Tables[0].ToList<SVDetailDV1>();
                return detail;
            }
            return result;
        }
    }
}
