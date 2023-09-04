using Genbyte.Component.Voucher;

namespace Voucher.SVTran_HDF
{
    public class SVBillModel:DetailEntity
    {
        public string bh_mau_hd { get; set; }
        public string bh_so_seri { get; set; }
        public DateTime? bh_ngay_hd { get; set; }
        public DateTime? bh_ngay_ky { get; set; }
        public string bh_so_hd { get; set; }
        public string bh_status { get; set; }
        public string bh_ma_so_thue { get; set; }
        public string bh_ma_tra_cuu { get; set; }
        public string bh_id_giaodich { get; set; }
        public string tl_mau_hd { get; set; }
        public string tl_so_seri { get; set; }
        public DateTime? tl_ngay_hd { get; set; }
        public DateTime? tl_ngay_ky { get; set; }
        public string tl_so_hd { get; set; }
        public string tl_status { get; set; }
        public string tl_ma_so_thue { get; set; }
        public string tl_ma_tra_cuu { get; set; }
        public string tl_id_giaodich { get; set; }
    }
}
