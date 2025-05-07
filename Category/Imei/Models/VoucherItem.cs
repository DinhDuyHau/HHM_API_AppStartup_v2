namespace Imei
{
    public class VoucherItem 
    {
        public string stt_rec { get; set; }
        public string ma_ct { get; set; }
        public string so_ct { get; set; }
        public DateTime? ngay_ct { get; set; }
        public string ma_dvcs { get; set; }
        public string ma_cuahang { get; set; }
        public string ma_ca { get; set; }
        public string status { get; set; }
        public string dien_giai { get; set; }
        public string ma_kh { get; set; }
        public string ong_ba { get; set; }
        public string ten_kh { get; set; }
        public string ten_ongba { get; set; }
        public string ma_gd { get; set; }
        public string ma_nt { get; set; }
        public decimal ty_gia { get; set; }
        public string loai_ct { get; set; }
        public string ma_thue { get; set; }
        public decimal t_so_luong { get; set; }
        public decimal t_tien_nt2 { get; set; }
        public decimal t_thue_nt { get; set; }
        public decimal t_tt_nt { get; set; }

        public bool cod_yn { get; set; }

        public bool freedelivery_yn { get; set; }

        public string so_dh_vc { get; set; }
        public string ma_nvvc { get; set; }
        public string ma_van_don { get; set; }

        public IList<VoucherDetail> details { get; set; }
    }
}
