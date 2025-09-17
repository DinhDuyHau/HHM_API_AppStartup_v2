using Genbyte.Component.Voucher;

namespace Voucher.CDTran_HT1
{
    public class VoucherItem : VoucherEntity
    {
        public DateTime? ngay_lct { get; set; }

        public string ma_gd { get; set; }

        public string ma_nvbh { get; set; }

        public string dien_giai { get; set; }

        public string ly_do_hoan { get; set; }

        public string ly_do_huy { get; set; }

        public string ma_ft { get; set; }

        public decimal tien_hoan { get; set; }

        public decimal tien_hoan_nt { get; set; }

        public string stt_rec_tt { get; set; }

        public string stt_rec0tt { get; set; }

        public string ma_ct_tt { get; set; }

        public string so_ct_tt { get; set; }

        public DateTime? ngay_ct_tt { get; set; }

        public string ma_thanhtoan { get; set; }

        public string ma_kh { get; set; }

        public decimal tien { get; set; }

        public decimal tien_nt { get; set; }

        public string terminal_id { get; set; }

        public string ref_code { get; set; }

        public bool bank_success_yn { get; set; }

        public DateTime? ngay_duyet { get; set; }

        public DateTime? ngay_huy { get; set; }

    }
}
