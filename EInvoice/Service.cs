using Genbyte.Base.CoreLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Genbyte.Sys.Common;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Genbyte.Sys.AppAuth;

namespace EInvoice
{
    public class Service: CoreService
    {
        private string baseUrl { get; set; } = "";
        public Service(IConfiguration configuration) {
            this.baseUrl = configuration["EInvoice:EInvoiceService"];
        }

        public async Task<object> CreateDraft(VoucherEntity voucher)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "CreateDraft";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }
        public async Task<object> CreateDraft(string voucherId, string ma_ct)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(voucherId) || !core_service.IsSQLInjectionValid(ma_ct))
                throw new Exception(ApiReponseMessage.Error_InputData);
            string sql = $"EXEC Genbyte$Voucher$GetById '{voucherId}', '{ma_ct}'";
            DataSet ds = core_service.ExecSql2DataSet(sql);
            VoucherEntity voucher = new VoucherEntity();
            if (ds != null && ds.Tables.Count >= 2)
            {
                voucher = ds.Tables[0].ToList<VoucherEntity>().FirstOrDefault();
                voucher.details = ds.Tables[1].ToList<VoucherDetail>();
                // Kiểm tra xem voucher đã có imei chưa
                var flag = true;
                voucher.details.ToList().ForEach(item =>
                {
                    if(item.ma_imei == null || item.so_luong != item.ma_imei.Split(",").Length)
                    {
                        flag = false;
                    }
                });
                if(flag == false)
                {
                    responseInfo.description = "lbl_voucher_not_save";
                    return responseInfo;

                }
                if (voucher.ma_thue == null)
                    voucher.ma_thue = "0";
            }
            
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "CreateDraft";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }

        public async Task<object> UpdateDraft(VoucherEntity voucher)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "UpdateDraft";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }
        public async Task<object> GetInvoicePDF(VoucherEntity voucher)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "GetInvoicePDF";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }


        public async Task<object> GetInvoicePDF(string voucherId,  string ma_ct)
        {
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(voucherId) || !core_service.IsSQLInjectionValid(ma_ct))
                throw new Exception(ApiReponseMessage.Error_InputData);
            string sql = $"EXEC Genbyte$Voucher$GetById '{voucherId}', '{ma_ct}'";
            DataSet ds = core_service.ExecSql2DataSet(sql);
            VoucherEntity voucher = new VoucherEntity();
            if (ds != null && ds.Tables.Count >= 2)
            {
                voucher = ds.Tables[0].ToList<VoucherEntity>().FirstOrDefault();
                var detail = ds.Tables[1].ToList<VoucherDetail>().ToList();
                detail.ForEach(x =>
                {
                    if (x.ma_imei == null)
                    {
                        x.ma_imei = "";
                    }
                });
                voucher.details = detail;
                if (voucher.ma_thue == null)
                    voucher.ma_thue = "0";
            }
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "GetInvoicePDF";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }

        public async Task<object> GetPublishedInv(VoucherEntity voucher)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "GetPublishedInv";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        SavePublishInv(responseBody, voucher.ma_ct, voucher.ngay_ct, voucher.ma_kh, voucher.dien_giai);
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }
        public async Task<bool> CheckPublishedInv(string voucherId, string ma_ct)
        {
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(voucherId) || !core_service.IsSQLInjectionValid(ma_ct))
                throw new Exception(ApiReponseMessage.Error_InputData);
            string sql = $"EXEC Genbyte$Voucher$GetById '{voucherId}', '{ma_ct}'";
            DataSet ds = core_service.ExecSql2DataSet(sql);
            VoucherEntity voucher = new VoucherEntity();
            if (ds != null && ds.Tables.Count >= 2)
            {
                voucher = ds.Tables[0].ToList<VoucherEntity>().FirstOrDefault();
                voucher.details = ds.Tables[1].ToList<VoucherDetail>();
                if (voucher.ma_thue == null)
                    voucher.ma_thue = "0";
            }
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "GetPublishedInv";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        var result = JsonConvert.DeserializeObject<Response>(responseBody);
                        var item = result.d;
                        // Nếu đủ thông tin thì đã phát hành
                        if (!string.IsNullOrEmpty(item.voucherId) && string.IsNullOrEmpty(item.errorCode) && item.result != null
                            && !string.IsNullOrEmpty(item.result.invoiceNo) && item.result.signedDate != null && !string.IsNullOrEmpty(item.result.reservationCode))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return false;
            }

            return false;
        }
        public async Task<object> GetPublishedInv(string voucherId, string ma_ct)
        {
            CoreService core_service = new CoreService();

            //check sql injection
            if (!core_service.IsSQLInjectionValid(voucherId) || !core_service.IsSQLInjectionValid(ma_ct))
                throw new Exception(ApiReponseMessage.Error_InputData);
            string sql = $"EXEC Genbyte$Voucher$GetById '{voucherId}', '{ma_ct}'";
            DataSet ds = core_service.ExecSql2DataSet(sql);
            VoucherEntity voucher = new VoucherEntity();
            if (ds != null && ds.Tables.Count >= 2)
            {
                voucher = ds.Tables[0].ToList<VoucherEntity>().FirstOrDefault();
                voucher.details = ds.Tables[1].ToList<VoucherDetail>();
                if (voucher.ma_thue == null)
                    voucher.ma_thue = "0";
            }
            ResponseInfo responseInfo = new ResponseInfo();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = baseUrl + "/" + "GetPublishedInv";
                    Request request = new Request();
                    request.voucherInfo = voucher;
                    string jsonData = JsonConvert.SerializeObject(request);
                    // Tạo nội dung HTTP
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //request.Headers.Add("Token", "c00f695f54df5caebd7a19bb37c98ca3d9a732a0");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ghi log
                    await LogRequest.Log(url, null, JsonConvert.SerializeObject(request, Formatting.None), "log_rq_hddt", responseBody, httpClient.DefaultRequestHeaders);

                    if (response.IsSuccessStatusCode)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng
                        //Response result = JsonConvert.DeserializeObject<Response>(responseBody);
                        var flag = SavePublishInv(responseBody, voucher.ma_ct, voucher.ngay_ct, voucher.ma_kh, voucher.dien_giai);
                        if(!flag)
                        {
                            responseInfo.description = "Không thể lấy thông tin hoá đơn";
                            responseInfo.errorCode = "cannot_get_einvoice";
                            Response rp = new Response();
                            rp.d = responseInfo;
                            return JsonConvert.SerializeObject(rp);
                        }
                        return responseBody;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                responseInfo.description = e.Message;
                return responseInfo;
            }

            return responseInfo;
        }
        public bool SavePublishInv(string jsonInvoice, string ma_ct, DateTime? ngay_ct, string ma_kh, string ghi_chu)
        {
            //Response response = JsonConvert.DeserializeObject<Response>(res);
            var result = JsonConvert.DeserializeObject<Response>(jsonInvoice);
            var item = result.d;

            // Nếu đủ thông tin thì cập nhật lại vào hoá đơn
            if (!string.IsNullOrEmpty(item.voucherId) && string.IsNullOrEmpty(item.errorCode)
                        && item.result != null && !string.IsNullOrEmpty(item.result.invoiceNo) && item.result.signedDate != null && !string.IsNullOrEmpty(item.result.reservationCode))
            {
                string sql = "EXEC GENBYTE$EInvoice$MappingPublishedInv @supplierId, @voucherId, @voucherCode, @customerCode, @note, @transactionId, @reservationCode, @buyerTaxCode, @invoiceNo, @invoiceForm, @invoiceSerial, @invoiceDate, @signedDate, @status_v, @status_e, @userId";
                List<SqlParameter> paras = new List<SqlParameter>();

                #region add parameters
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@supplierId",
                    SqlDbType = SqlDbType.VarChar,
                    Value = ""
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@voucherId",
                    SqlDbType = SqlDbType.Char,
                    Value = item.voucherId
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@voucherCode",
                    SqlDbType = SqlDbType.Char,
                    Value = ma_ct
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@customerCode",
                    SqlDbType = SqlDbType.Char,
                    Value = ma_kh
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@note",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = ghi_chu
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@transactionId",
                    SqlDbType = SqlDbType.VarChar,
                    Value = string.IsNullOrEmpty(item.result.transactionID) ? "" : item.result.transactionID
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@reservationCode",
                    SqlDbType = SqlDbType.VarChar,
                    Value = item.result.reservationCode ?? ""
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@buyerTaxCode",
                    SqlDbType = SqlDbType.VarChar,
                    Value = item.result.buyerTaxCode
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@invoiceNo",
                    SqlDbType = SqlDbType.VarChar,
                    Value = string.IsNullOrEmpty(item.result.invoiceNo.Remove(0, item.result.invoiceSerial.Trim().Length))? "1": item.result.invoiceNo.Remove(0, item.result.invoiceSerial.Trim().Length)
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@invoiceForm",
                    SqlDbType = SqlDbType.VarChar,
                    Value = item.result.invoiceForm
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@invoiceSerial",
                    SqlDbType = SqlDbType.VarChar,
                    Value = item.result.invoiceSerial
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@invoiceDate",
                    SqlDbType = SqlDbType.DateTime,
                    Value = ngay_ct
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@signedDate",
                    SqlDbType = SqlDbType.DateTime,
                    Value = item.result.signedDate ?? null
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@status_v",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = item.result.status_v
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@status_e",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = item.result.status_e
                });
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@userId",
                    SqlDbType = SqlDbType.Int,
                    Value = 1
                });
                #endregion
                this.ExecuteNonQuery(sql, paras);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckValidVoucherStatus(string stt_rec, string ma_ct)
        {
            bool check = false;
            string sql = "EXEC Genbyte$EInvoice$CheckValidVoucherStatus @stt_rec, @ma_ct";
            List<SqlParameter> paras = new List<SqlParameter>();

            #region add parameters
            paras.Add(new SqlParameter()
            {
                ParameterName = "@stt_rec",
                SqlDbType = SqlDbType.Char,
                Value = stt_rec
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ma_ct",
                SqlDbType = SqlDbType.Char,
                Value = ma_ct
            });
            #endregion
            try
            {
                DataSet ds = this.ExecSql2DataSet(sql, paras, ConnectType.App);
                if(ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    check = Convert.ToBoolean(ds.Tables[0].Rows[0]["is_valid"]);
                }
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Shop, $"EInvoice.CheckValidVoucherStatus / {stt_rec}", ex);
            }
            return check;
        }

    }
 }
