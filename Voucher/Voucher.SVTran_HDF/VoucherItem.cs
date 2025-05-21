using Genbyte.Component.Voucher;

namespace Voucher.SVTran_HDF
{
    public class VoucherItem : VocherItemBase
    {
        public decimal t_ck { get; set; }
        public decimal t_ck_nt { get; set; }
        public decimal t_da_tra { get; set; }
        public decimal t_con_no { get; set; }
        public decimal tien_dat_coc { get; set; }
        public decimal t_tien_tnk { get; set; }
        public string ma_nvvc { get; set; }
        public DateTime? fdate2 { get; set; }

        public string stt_rec_hd { get; set; }

        public bool tra_lai_cod { get; set; }
        public string so_dh_vc { get; set; }
        public string ma_van_don { get; set; }

        public bool tra_lai_freedelivery { get; set; }
    }
}
