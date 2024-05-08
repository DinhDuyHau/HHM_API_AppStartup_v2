using Genbyte.Component.Report.Model;

namespace Report.RptEndofShiftReport
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public string ma_ca { get; set; } = "";
    }
}