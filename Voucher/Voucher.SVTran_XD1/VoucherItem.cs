using Genbyte.Component.Voucher;

namespace Voucher.SVTran_XD1
{
    public class VoucherItem : VocherItemBase
    {
        public decimal t_con_no { get; set; }
        public decimal t_ck { get; set; }
        public decimal t_ck_nt { get; set; }
        public decimal tien_dat_coc { get; set; }
        public decimal t_da_tra { get; set; }
        public string ma_nvvc { get; set; }
        public string ma_nk { get; set; }
        public string so_seri { get; set; }
        public string email_nhan_key { get; set; }

        public decimal t_cp_khac { get; set; }
        public decimal t_cp_khac_nt { get; set; }

        public decimal fqty1 { get; set; }

        public string ma_loaivc { get; set; }

        public string so_dh_vc { get; set; }

        public string ma_van_don { get; set; }

        public decimal tien_phi_cod { get; set; }

        public string ghi_chu_gh { get; set; }
    }
}
