namespace VoucherWebsite.Model
{
    public class VoucherPayloadV2
    {
        public string Voucher { get; set; } // mã voucher
        public string Member { get; set; } // mã hạng
        public string Phone { get; set; } // mã khách
        public string Stock { get; set; } // cửa hàng
        public List<string> SKU { get; set; } // danh sách mã vật tư
    }
}
