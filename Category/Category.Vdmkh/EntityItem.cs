using Genbyte.Sys.Common;

namespace Category.Vdmkh
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_kh { get; set; }

        public string ten_kh { get; set; }

        public string ten_kh2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string dia_chi { get; set; }

        public string ma_so_thue { get; set; }
        public string gioi_tinh { get; set; }
        public string dien_thoai { get; set; }
        public string email_cn { get; set; }
        public DateTime? ngay_sinh { get; set; }
        public string ma_tinh { get; set; }
        public string ma_quan { get; set; }
        public string ma_phuong { get; set; }

        public bool kh_yn { get; set; }

        public bool cc_yn { get; set; }

        public bool nv_yn { get; set; }
        public string image { get; set; }
        public string hoadon_mst { get; set; }
        public string hoadon_tenkh { get; set; }
        public string hoadon_diachi { get; set; }
        public string hoadon_email { get; set; }
        public decimal do_tuoi { get; set; }
        public string ten_tinh { get; set; }
        public string ten_quan { get; set; }
        public string ten_phuong { get; set; }

    }
}
