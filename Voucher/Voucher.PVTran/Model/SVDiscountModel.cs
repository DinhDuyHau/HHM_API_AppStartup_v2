using Genbyte.Component.Voucher;

namespace Voucher.PVTran
{
    public class SVDiscountModel : DetailEntity
    {
        public string stt_rec { get; set; }
        public string stt_rec0 { get; set; }
        public string ma_ct { get; set; }
        public DateTime? ngay_ct { get; set; }
        public string so_ct { get; set; }
        public string dien_giai { get; set; }
        public decimal thue_suat { get; set; }
        public decimal tien { get; set; }
        public string ma_td1 { get; set; }
        public string ma_td2 { get; set; }
        public string ma_td3 { get; set; }
        public decimal sl_td1 { get; set; }
        public decimal sl_td2 { get; set; }
        public decimal sl_td3 { get; set; }
        public DateTime? ngay_td1 { get; set; }
        public DateTime? ngay_td2 { get; set; }
        public DateTime? ngay_td3 { get; set; }
        public string gc_td1 { get; set; }
        public string gc_td2 { get; set; }
        public string gc_td3 { get; set; }
    }
}
