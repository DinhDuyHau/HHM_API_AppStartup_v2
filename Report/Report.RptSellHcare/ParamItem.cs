using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptSellHcare
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string ma_cuahang { get; set; } = "";

        public string ma_dv { get; set; } = "";

        public string ma_loai_dv { get; set; } = "";

        public string nh_dv1 { get; set; } = "";

        public string nh_dv2 { get; set; } = "";

        public string nh_dv3 { get; set; } = "";

    }
}
