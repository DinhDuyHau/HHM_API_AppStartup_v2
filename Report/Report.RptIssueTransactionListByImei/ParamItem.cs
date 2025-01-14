using Genbyte.Component.Report.Model;

namespace Report.RptIssueTransactionListByImei
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_cuahang { get; set; } = "";
        public string ma_vt { get; set; } = "";
        public string ma_imei { get; set; } = "";
        public string ma_kh { get; set; } = "";
        public string ma_kho { get; set; } = "";
        public string kho_hang_dc { get; set; } = "";
        public string loai_du_lieu { get; set; } = "2";
        public string nh_vt1 { get; set; } = "";
        public string nh_vt2 { get; set; } = "";
        public string nh_vt3 { get; set; } = "";
        public int mau_bc { get; set; } = 10;

    }
}