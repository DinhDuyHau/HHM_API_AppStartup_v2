namespace VoucherWebsite.Model
{
    public class VoucherPayload
    {
        public string Voucher { get; set; } // mã voucher
        public List<string> SKU { get; set; } // danh sách mã vật tư
    }
}
