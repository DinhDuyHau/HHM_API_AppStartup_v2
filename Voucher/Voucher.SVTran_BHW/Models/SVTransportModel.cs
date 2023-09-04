using Genbyte.Component.Voucher;

namespace Voucher.SVTran_BHW
{
    public class SVTransportModel : DetailEntity
    {
        public string ma_loaivc { get; set; }
        public string so_dh_vc { get; set; }
        public string ma_van_don { get; set; }
        public decimal tien_phi_cod { get; set; }
        public string ma_nv_giao { get; set; }
        public string ghi_chu_gh { get; set; }
    }
}
