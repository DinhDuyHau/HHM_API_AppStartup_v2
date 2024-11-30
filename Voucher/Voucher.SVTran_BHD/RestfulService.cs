using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Genbyte.Sys.Common;

namespace Voucher.SVTran_BHD
{
    public class RestfulService
    {
        public static WsResponse SendRequest(string actionUrl, HttpMethod method, string jsonBody, Dictionary<string, string> headers = null)
        {
            WsResponse response = new WsResponse();

            //long ticks = DateTime.Now.Ticks;
            try
            {
                HttpClient client = new HttpClient();
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(method, actionUrl);
                    try
                    {
                        if (headers != null && headers.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> header in headers)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }
                        }

                        if (!string.IsNullOrEmpty(jsonBody))
                        {
                            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        }

                        Task<HttpResponseMessage> task = Task.Run(() => client.SendAsync(request, HttpCompletionOption.ResponseContentRead));
                        task.Wait();

                        string result = task.Result.Content != null ? task.Result.Content.ReadAsStringAsync().Result : "";
                        if (!string.IsNullOrEmpty(result))
                            response = Newtonsoft.Json.JsonConvert.DeserializeObject<WsResponse>(result);

                    }
                    finally
                    {
                        if (request != null)
                        {
                            ((IDisposable)request).Dispose();
                        }
                    }
                }
                finally
                {
                    if (client != null)
                    {
                        ((IDisposable)client).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Insert("MOBIFONE", actionUrl, ex);
                response.code = "HTTP_500";
                response.message = "Không thể kết nối webservice";
            }

            return response;
        }
    }
}
