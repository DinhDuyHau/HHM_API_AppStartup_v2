using Genbyte.Component.Report.Model;

namespace Report.RptLoanerWarranty
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_cuahang { get; set; } = "";
        public string so_ct1 { get; set; } = "";
        public string so_ct2 { get; set; } = "";
        public string ma_dvbh { get; set; } = "";
        public string ma_nv { get; set; } = "";
        public string ma_doi_tac { get; set; } = "";
        public string ma_vt { get; set; } = "";
        public string mau_bc { get; set; } = "";
        public string nh_vt1 { get; set; } = "";

        public string nh_vt2 { get; set; } = "";

        public string nh_vt3 { get; set; } = "";
        public string nh_vt4 { get; set; } = "";

        public string nh_vt5 { get; set; } = "";
        public string ma_nganh { get; set; } = "";
    }
}
