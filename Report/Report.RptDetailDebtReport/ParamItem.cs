using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptDetailDebtReport
{
    public class ParamItem : ParamItemBase
    {
        public DateTime ngay_ht1 { get; set; }
        public DateTime ngay_ht2 { get; set; }
        public DateTime ngay_ct1 { get; set; }
        public DateTime ngay_ct2 { get; set; }
        public DateTime ngay_tt { get; set; }
        public string ma_cuahang { get; set; } = "";
        public string ma_kh { get; set; } = "";
        public string ma_nh1 { get; set; } = "";
        public string ma_nh2 { get; set; } = "";
        public string ma_nh3 { get; set; } = "";


    }
}
