using Genbyte.Component.Voucher;

namespace Voucher.SVTran_BHC
{
    public class SVEcommerceModel : DetailEntity
    {
        public string ma_kh_tmdt { get; set; }
        public string ma_dh { get; set; }
        public decimal doanh_thu { get; set; }
        public decimal giam_gia_tmdt { get; set; }
        public decimal giam_gia_hhm { get; set; }
        public decimal thanh_tien { get; set; }
        public string ma_dvvc { get; set; }
        public DateTime? ngay_nhan_hang { get; set; }
        public decimal phi_vc { get; set; }
        public string nguoi_nhan_hang { get; set; }
    }
}
