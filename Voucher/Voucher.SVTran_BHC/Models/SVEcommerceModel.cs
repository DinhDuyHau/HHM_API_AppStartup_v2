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

        public string ma_van_don { get; set; }

        public decimal tien_phi_01 { get; set; }

        public decimal tien_phi_02 { get; set; }

        public decimal tien_phi_03 { get; set; }

        public decimal tien_phi_04 { get; set; }

        public decimal tien_phi_05 { get; set; }

        public decimal tien_phi_06 { get; set; }

        public decimal tien_phi_07 { get; set; }

        public decimal tien_phi_08 { get; set; }

        public decimal tien_phi_09 { get; set; }

        public decimal tien_phi_10 { get; set; }

        public decimal phi_hoang_ha { get; set; }


    }
}
