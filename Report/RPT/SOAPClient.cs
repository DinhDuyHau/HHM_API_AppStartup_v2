using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Report
{
    public class SoapClient
    {
        private readonly HttpClient _httpClient;

        public SoapClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> CallSoapService(string soapEndpointUrl, string soapXmlBody)
        {
            try
            {
                // Xây dựng nội dung yêu cầu SOAP
                var soapContent = new StringContent(soapXmlBody, Encoding.UTF8, "text/xml");

                // Gửi yêu cầu SOAP đến địa chỉ URL của dịch vụ web
                var response = await _httpClient.PostAsync(soapEndpointUrl, soapContent);

                // Đọc nội dung phản hồi từ dịch vụ web
                var responseContent = await response.Content.ReadAsStringAsync();

                // Trả về kết quả
                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                // Xử lý lỗi khi gửi yêu cầu
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public string ExtractExportToBase64Result(string soapResponse, string tagName)
        {
            try
            {
                // Sử dụng XmlDocument để phân tích cú pháp XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(soapResponse);

                // Lấy phần tử ExportToBase64Result từ XML
                XmlNodeList nodes = xmlDoc.GetElementsByTagName(tagName);
                if (nodes.Count > 0)
                {
                    return nodes[0].InnerText; // Trả về giá trị của ExportToBase64Result
                }
                else
                {
                    return null; // Không tìm thấy phần tử ExportToBase64Result
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
