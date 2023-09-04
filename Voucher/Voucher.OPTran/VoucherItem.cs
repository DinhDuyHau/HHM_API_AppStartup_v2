using Genbyte.Component.Voucher;

namespace Voucher.OPTran
{
    public class VoucherItem : VoucherEntity
    {
        public string ma_gd { get; set; }
        public string loai_ct { get; set; }
        public string ma_loaigd { get; set; }
        public string tk { get; set; }

        public string dien_giai { get; set; }
        public string ma_kh { get; set; }
        public string ong_ba { get; set; }
        public string dia_chi { get; set; }
        public decimal ty_gia { get; set; }
        public string ma_nt { get; set; }
        public decimal t_tien { get; set; }
        public decimal t_tien_nt { get; set; }
        public decimal t_thue { get; set; }
        public decimal t_thue_nt { get; set; }
        public decimal t_tt { get; set; }
        public decimal t_tt_nt { get; set; }
        public DateTime? ngay_lct { get; set; }
    }
}
