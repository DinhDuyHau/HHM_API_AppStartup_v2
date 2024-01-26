using Genbyte.Component.Voucher;

namespace Voucher.SVTran_DXA
{
    public class VoucherItem : VocherItemBase
    {
        public decimal t_con_no { get; set; }
        public decimal t_ck { get; set; }
        public decimal t_ck_nt { get; set; }
        public decimal tien_dat_coc { get; set; }
        public decimal t_da_tra { get; set; }
        public string ma_nvvc { get; set; }
        public string ma_nvbh { get; set; }
        public DateTime? ngay_ct { get; set; }
        public DateTime? ngay_ct0 { get; set; }
        public DateTime? ngay_ct2 { get; set; }
        public DateTime? ngay_ct3 { get; set; }
        public DateTime? ngay_hl { get; set; }
        public DateTime? ngay_lo { get; set; }
        public string s1 { get; set; }

    }
}
