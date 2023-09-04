using Genbyte.Component.Voucher;

namespace Voucher.SVTran_BHB
{
    public class SVTransModel: DetailEntity
    {
        public string stt_rec_hd { get; set; }
        public string so_ct_hd { get; set; }
        public DateTime? ngay_ct_hd { get; set; }
        public string ten_kh { get; set; }
        public string dia_chi { get; set; }
        public string ma_loaivc { get; set; }
        public string so_dh_vc { get; set; }
        public string ma_van_don { get; set; }
        public decimal tien_phi_cod { get; set; }
        public string ma_nv_giao { get; set; }
        public string ghi_chu_gh { get; set; }
        public string ma_co { get; set; }
        public string file_co { get; set; }
        public string ma_cq { get; set; }
        public string file_cq { get; set; }
    }
}
