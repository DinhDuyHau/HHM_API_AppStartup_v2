using Genbyte.Sys.Common;

namespace Category.Dmkh
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

        public bool kh_yn { get; set; } = true;

        public bool cc_yn { get; set; } = true;

        public bool nv_yn { get; set; } = true;
        public bool vdt_yn { get; set; }
        public string image { get; set; }
        public string hoadon_mst { get; set; }
        public string hoadon_tenkh { get; set; }
        public string hoadon_diachi { get; set; }
        public string hoadon_email { get; set; }
        public decimal do_tuoi { get; set; }

        public string tk_nh { get; set; }
        public string ngan_hang { get; set; }
        public string status { get; set; } = "1";

        //Mã giao dịch của khách hàng
        public string ma_td2 { get; set; }

        //Tên giao dịch của khách hàng
        public string ten_cty { get; set; }
        // Nhóm 3
        public string nh_kh3 { get; set; }
    }
}
