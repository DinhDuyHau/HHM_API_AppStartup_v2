using Genbyte.Sys.Common;

namespace Category.Vctckhangtang
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_ck { get; set; }
        [IsPrimary]
        public decimal rec { get; set; }

        public string ten_ck { get; set; }

        public DateTime? ngay_bd { get; set; }
        public DateTime? ngay_kt { get; set; }
        public string ma_ct { get; set; }

        public string ma_dvcs { get; set; }

        public string ma_cuahang { get; set; }

        public string ma_kho { get; set; }

        public string ma_kh { get; set; }

        public string nh_kh1 { get; set; }

        public string nh_kh2 { get; set; }

        public string nh_kh3 { get; set; }

        public int uu_tien { get; set; }

        public decimal tien_tu { get; set; }

        public decimal tien_den { get; set; }

        public string status { get; set; }

        public string ma_vt { get; set; }

        //public decimal so_luong { get; set; }

        public string ma_vt_tang { get; set; }

        public decimal sl_tang { get; set; }

        public string ma_vt_tt { get; set; }

        public decimal sl_vt_tt { get; set; }

        public decimal tien_qd { get; set; }

        public string nh_vt1 { get; set; }

        public string nh_vt2 { get; set; }

        public string nh_vt3 { get; set; }

        public string nh_vt4 { get; set; }

        public string ma_dv { get; set; }

    }
}
