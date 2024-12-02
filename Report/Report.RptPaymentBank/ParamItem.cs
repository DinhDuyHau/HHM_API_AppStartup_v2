using Genbyte.Component.Report.Model;

namespace Report.RptPaymentBank
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string so_ct1 { get; set; } = "";

        public string so_ct2 { get; set; } = "";

        public string ma_ca { get; set; } = "";

        public string ma_cuahang { get; set; } = "";

        public string tk { get; set; } = "";

        public string ma_kh { get; set; } = "";


        public string ma_khqttg { get; set; } = "";

        public string ma_khvdt { get; set; } = "";

        public bool ck_yn { get; set; } = true;

        public string tk_nh { get; set; } = "";

        public bool qt_yn { get; set; } = true;

        public string ma_posqt { get; set; } = "";

        public bool qttg_yn { get; set; } = true;

        public string ma_posqttg { get; set; } = "";

        public bool vdt_yn { get; set; } = true;

        public bool vnpay_yn { get; set; } = true;

    }
}