using Genbyte.Component.Report.Model;

namespace Report.RptSaleByImei
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }


        public string ma_ca { get; set; } = "";

        public string ma_kho { get; set; } = "";

        public string ma_vt { get; set; } = "";


        public string ma_nvbh { get; set; } = "";

    }
}