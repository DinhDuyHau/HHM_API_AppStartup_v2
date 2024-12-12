using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptRetailPrice
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public string ma_cuahang { get; set; }

        public string ma_vt { get; set; }

        public string nh_vt3 { get; set; }
    }
}
