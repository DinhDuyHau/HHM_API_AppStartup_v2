using Genbyte.Base.CoreLib;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voucher.SVTran_BHD.Models;
using Genbyte.Component.Voucher;
using Newtonsoft.Json;
using Genbyte.Sys.Common;

namespace Voucher.SVTran_BHD
{
    public class MobifoneService
    {
        private MobiAccountInfo account = new MobiAccountInfo();

        private const int M_THUE_SUAT = 10;

        public string ApiUrl { get; set; } = "";

        public MobifoneService(IConfiguration configuration)
        {
            account = new MobiAccountInfo();

            account.user_id = configuration["MobifoneAccount:uid"]; 
            account.password = configuration["MobifoneAccount:pwd"];
            account.api_url = configuration["MobifoneAccount:uri"];
            account.parner_id = configuration["MobifoneAccount:parnerid"];
        }

        public void InsertLog(string url, string message)
        {
            long ticks = DateTime.Now.Ticks;
            string sql = "INSERT INTO log_mobifone (TicksId, CreatedOn, Url, Message) VALUES (@ticks, GETDATE(), @url, @message)";

            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = "@ticks",
                SqlDbType = SqlDbType.BigInt,
                Value = ticks
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@url",
                SqlDbType = SqlDbType.NVarChar,
                Value = url
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@message",
                SqlDbType = SqlDbType.NVarChar,
                Value = message
            });
            CoreService service = new CoreService();
            service.ExecuteNonQuery(sql, paras);
        }

        public WsResponse SendInvoiceToMBF(VoucherItem voucher, string detail_para_name)
        {
            WsResponse res = new WsResponse()
            {
                code = "",
                message = ""
            };
            if (voucher == null) return res;

            //thông tin khách hàng
            string ten_kh = "";
            string dia_chi = "";
            string dien_thoai = "";
            IList<InvItemBHD> inv_items = null;

            //thông tin cửa hàng
            string shop_code = "";

            try
            {
                CoreService service = new CoreService();
                string sql = "exec Genbyte$Voucher$BHD$CreateInvoiceMobifone @stt_rec";
                List<SqlParameter> paras = new List<SqlParameter>();
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@stt_rec",
                    SqlDbType = SqlDbType.Char,
                    Value = voucher.stt_rec
                });
                DataSet ds = service.ExecSql2DataSet(sql, paras);

                if (ds != null && ds.Tables.Count == 2 && ds.Tables[0].Rows.Count > 0 && ds.Tables[1].Rows.Count > 0)
                {
                    ten_kh = ds.Tables[0].Rows[0]["ten_kh"].ToString().Trim();
                    dia_chi = ds.Tables[0].Rows[0]["dia_chi"].ToString().Trim();
                    dien_thoai = ds.Tables[0].Rows[0]["dien_thoai"].ToString().Trim();
                    shop_code = ds.Tables[0].Rows[0]["ma_shopcode"].ToString().Trim();

                    inv_items = ds.Tables[1].ToList<InvItemBHD>();
                }
            }
            catch(Exception ex)
            {
                Logger.Insert("MOBIFONE", "SendInvoiceToMBF", ex);
            }

            if (string.IsNullOrEmpty(ten_kh) || string.IsNullOrEmpty(shop_code))
            {
                res.message = "err_customer_shopcode";
                return res;
            }

            MobiInvoice mobi_invoice = new MobiInvoice();
            mobi_invoice.transCode = voucher.stt_rec;
            mobi_invoice.partnerType = this.account.parner_id;
            mobi_invoice.transDate = voucher.ngay_ct!.Value.ToString("dd/MM/yyyy");
            mobi_invoice.vat = M_THUE_SUAT;
            mobi_invoice.custName = ten_kh;
            mobi_invoice.address = dia_chi;
            mobi_invoice.telNumber = dien_thoai;
            mobi_invoice.tin = "";
            mobi_invoice.shopCode = shop_code;

            if (inv_items != null && inv_items.Count > 0) foreach (InvItemBHD item in inv_items)
                {
                    MobiInvDetail mobi_detail = new MobiInvDetail();
                    mobi_detail.goodCode = item.ma_vt;
                    mobi_detail.goodName = item.ten_vt;
                    mobi_detail.amount = item.gia_vat;
                    mobi_detail.discount = item.ck;
                    mobi_detail.imei = item.ma_imei;
                    mobi_detail.attachStatus = item.loai_hang;
                    mobi_detail.entryPrice = item.gia_nhap;

                    mobi_invoice.lstItem.Add(mobi_detail);
                }

            string json_body = JsonConvert.SerializeObject(mobi_invoice);

            this.ApiUrl = account.api_url + (account.api_url.EndsWith("/") ? "" : "/") + "create-sis-transaction";

            //header
            string auth_token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{account.user_id}:{account.password}"));
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", $"Basic {auth_token}");

            //add log before request
            InsertLog(this.ApiUrl, $"REQUEST ### {json_body}");

            //send request
            res = RestfulService.SendRequest(this.ApiUrl, System.Net.Http.HttpMethod.Post, json_body, header);

            //add result to log
            InsertLog(this.ApiUrl, $"SUCCESS ### RESPONSE: code = '{res.code}', message = '{res.message}' ### {json_body}");

            return res;
        }
    }
}
