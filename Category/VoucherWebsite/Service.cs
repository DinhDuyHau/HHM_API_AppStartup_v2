using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using VoucherWebsite.Model;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Http;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace VoucherWebsite
{
    public class Service
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<HttpResponseMessage> VoucherCheck(VoucherPayload payload)
        {
            var domain = "";
            var path = "";
            var token = "";

            CoreService service = new CoreService();
            string query = "select top 1 * from api_hhm where type = 'voucher_code' and name = 'vouchercheck'";
            var result = service.ExecSql2Dictionary(query, null);
            if (result != null && result.Count > 0)
            {
                domain = result[0]["domain"]?.ToString();
                path = result[0]["path"]?.ToString();
                token = result[0]["token"]?.ToString();
            }
            var url_request = "https://" + domain + path;

            var requestBody = new JObject
            {
                ["Voucher"] = payload.Voucher,
                ["SKU"] = JArray.FromObject(payload.SKU ?? new List<string>())
            };
            var content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("token", token);

            var response = await client.PostAsync(url_request, content);
            return response;
        }

        public static async Task<HttpResponseMessage> VoucherCheckV2(VoucherPayloadV2 payload)
        {
            var domain = "";
            var path = "";
            var token = "";

            CoreService service = new CoreService();
            string query = "select top 1 * from api_hhm where type = 'voucher_code' and name = 'vouchercheckv2'";
            var result = service.ExecSql2Dictionary(query, null);
            if (result != null && result.Count > 0)
            {
                domain = result[0]["domain"]?.ToString();
                path = result[0]["path"]?.ToString();
                token = result[0]["token"]?.ToString();
            }
            var url_request = "https://" + domain + path;

            //if(string.IsNullOrEmpty(payload.Member))
            //{
            //    payload.Member = "NEWMEMBER";
            //}

            var requestBody = new JObject
            {
                ["Voucher"] = payload.Voucher,
                ["Member"] = payload.Member,
                ["Phone"] = payload.Phone,
                ["Stock"] = payload.Stock,
                ["SKU"] = JArray.FromObject(payload.SKU ?? new List<string>())
            };
            var content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("token", token);

            var response = await client.PostAsync(url_request, content);
            return response;
        }

        public static string TryFormatJson(string raw)
        {
            try
            {
                var doc = JsonDocument.Parse(raw);
                return JsonSerializer.Serialize(doc, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                });
            }
            catch { return raw; }
        }
    }
}
