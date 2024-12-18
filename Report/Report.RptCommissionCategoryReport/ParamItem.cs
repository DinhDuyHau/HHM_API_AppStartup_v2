using Genbyte.Component.Report.Model;

namespace Report.RptCommissionCategoryReport
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_hoahong { get; set; } = "";
        public string ma_vt { get; set; } = "";
        public string ma_imei { get; set; } = "";
    }
}
