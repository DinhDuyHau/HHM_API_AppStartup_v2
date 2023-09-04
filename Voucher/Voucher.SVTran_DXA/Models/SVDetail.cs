using Genbyte.Component.Voucher;

namespace Voucher.SVTran_DXA
{
    public class SVDetail : DetailEntity
    {
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }

        public string ma_imei { get; set; }

        public string ma_kho { get; set; }

        public decimal so_luong { get; set; }

        public decimal gia_nt2 { get; set; }
        public decimal gia { get; set; }
        public decimal gia_ban { get; set; }
        public decimal gia_ck { get; set; }

        public decimal tien_nt2 { get; set; }
        public string dvt { get; set; }
        public string ma_thue { get; set; }
        public decimal thue_suat { get; set; }
        public decimal thue { get; set; }
        public decimal tt { get; set; }
        public decimal thanh_tien { get; set; }
        public decimal tien_thue { get; set; }
        public decimal tong_tien { get; set; }
        public string imei_mua { get; set; }
        public bool hang_km { get; set; }
        public bool no_km_yn { get; set; }
    }
}
