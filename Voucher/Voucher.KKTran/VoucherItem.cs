using Genbyte.Component.Voucher;

namespace Voucher.KKTran
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
        public DateTime? ngay_lct { get; set; }
        public string ma_vt { get; set; }
        public string nh_vt1 { get; set; }
        public string nh_vt2 { get; set; }
        public string nh_vt3 { get; set; }
        public string nh_vt4 { get; set; }
        public string ma_nvkk01 { get; set; }
        public string ma_nvkk02 { get; set; }
        public string ma_nvkk03 { get; set; }
    }
}
