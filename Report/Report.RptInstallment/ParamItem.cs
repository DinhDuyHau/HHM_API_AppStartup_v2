using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptInstallment
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_cuahang { get; set; } = "";
        public string ma_ca { get; set; } = "";
        public string ma_kh { get; set; } = "";
        public string ma_dvtg { get; set; } = "";
    }
}
