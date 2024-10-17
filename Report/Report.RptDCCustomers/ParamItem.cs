using Genbyte.Component.Report.Model;

namespace Report.RptDCCustomers
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string ma_kh { get; set; } = "";

        public string ma_cuahang { get; set; } = "";

    }
}