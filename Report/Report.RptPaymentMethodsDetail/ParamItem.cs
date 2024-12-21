using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptPaymentMethodsDetail
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string tu_so { get; set; } = "";
        public string den_so { get; set; } = "";
        public string ma_cuahang { get; set; } = "";
        public string ma_ca { get; set; } = "";
        public string ma_thanhtoan { get; set; } = "";
        public string ma_kho { get; set; } = "";
        public string ma_kh { get; set; } = "";
        public string ma_vc { get; set; } = "";
        public string ma_san { get; set; } = "";
        public string ma_thuho { get; set; } = "";
        public string ma_dvtg { get; set; } = ""; 

    }
}
