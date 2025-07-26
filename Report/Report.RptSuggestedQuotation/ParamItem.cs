using Genbyte.Component.Report.Model;

namespace Report.RptSuggestedQuotation
{
    public class ParamItem : ParamItemBase
    {
        public DateTime den_ngay { get; set; }
        public string ma_nh { get; set; } = "";
        public string ma_cuahang { get; set; } = "";
        public string ma_vt { get; set; } = "";
        public string nh_vt1 { get; set; } = "";
        public string nh_vt2 { get; set; } = "";
        public string nh_vt3 { get; set; } = "";
        public string nh_vt4 { get; set; } = "";
        public string nh_vt5 { get; set; } = "";
        public string ma_nganh { get; set; } = "";
    }
}
