using Genbyte.Component.Report.Model;

namespace Report.RptPaymentBank
{
    public class ParamItem : ParamItemBase
    {
        public DateTime ngay_ct1 { get; set; }

        public DateTime ngay_ct2 { get; set; }

        public string ma_ca { get; set; } = "";

        public string ma_cuahang { get; set; } = "";

        public string tk { get; set; } = "";

        public string ma_kh { get; set; } = "";


    }
}