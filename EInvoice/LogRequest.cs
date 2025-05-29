using Genbyte.Base.CoreLib;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EInvoice
{
    public class LogRequest
    {
        /// <summary>
        /// Ghi logs
        /// </summary>
        /// <param name="slug">Endpoint thực hiện</param>
        /// <param name="tablelog">Bảng log</param>
        /// <param name="header">Header gửi request</param>
        /// <param name="request">Raw body gửi request</param>
        /// <param name="response">Response trả ra từ request</param>
        public static void Insert(string slug, string tablelog, string header = "", string request = "", string response = "")
        {
            CoreService service = new CoreService();
            string sql = $@"insert into {tablelog} (slug, header, body, response, created_at) 
            values (@slug, @header, @request, @response, getdate())";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@slug",
                SqlDbType = SqlDbType.VarChar,
                Value = slug
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@header",
                SqlDbType = SqlDbType.NVarChar,
                Value = header
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@request",
                SqlDbType = SqlDbType.NVarChar,
                Value = request
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@response",
                SqlDbType = SqlDbType.NVarChar,
                Value = response
            });
            service.ExecuteNonQuery(sql, paras);
        }

        public static async Task Log<T>(string slug, HttpContext context, T model, string tablelog, string response = "", HttpRequestHeaders customHeaders = null)
        {
            StringBuilder headerLog = new StringBuilder();

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    string headerValue = string.Join(", ", header.Value);
                    headerLog.AppendLine($"{header.Key}: {headerValue}");
                }
            }
            else if (context != null)
            {
                foreach (var header in context.Request.Headers)
                {
                    string headerValue = string.Join(", ", header.Value.ToArray());
                    headerLog.AppendLine($"{header.Key}: {headerValue}");
                }
            }

            string requestBody;
            if (model is string str)
            {
                requestBody = str; // đã là JSON string gọn rồi
            }
            else
            {
                requestBody = JsonConvert.SerializeObject(model, new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.Default, // Giữ nguyên tiếng Việt
                    Formatting = Formatting.None
                });
            }

            Insert(slug, tablelog, headerLog.ToString(), requestBody, response);
        }
    }
}
