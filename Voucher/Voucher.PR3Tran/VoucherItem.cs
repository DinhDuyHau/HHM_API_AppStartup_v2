using Genbyte.Component.Voucher;

namespace Voucher.PR3Tran
{
    public class VoucherItem : VoucherEntity
    {
        public string dien_giai { get; set; }
        public string ong_ba { get; set; }
        public string ma_kh { get; set; }
        public string ma_gd { get; set; }
        public string ma_nt { get; set; }
        public decimal ty_gia { get; set; }
        public string loai_ct { get; set; }
        public decimal t_so_luong { get; set; }
        public decimal t_tien { get; set; }
        public decimal t_tien_nt { get; set; }
        public DateTime? ngay_lct { get; set; }
        public string ma_khon { get; set; }
        public string ma_kho { get; set; }
        public string ma_cuahang_n { get; set; }
    }
}
