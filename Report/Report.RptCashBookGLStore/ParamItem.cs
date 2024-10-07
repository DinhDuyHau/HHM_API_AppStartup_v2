using Genbyte.Component.Report.Model;

namespace Report.RptCashBookGLStore
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string ma_cuahang { get; set; } = "";

        public string ma_ca { get; set; } = "";

    }
}