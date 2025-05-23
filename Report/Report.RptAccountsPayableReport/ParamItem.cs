using Genbyte.Component.Report.Model;

namespace Report.RptAccountsPayableReport
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string ma_kh { get; set; } = "";

        public string ma_cuahang { get; set; } = "";

    }
}