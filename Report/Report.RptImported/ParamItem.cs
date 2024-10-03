using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptImported
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string so_ct1 { get; set; } = "";
        public string so_ct2 { get; set; } = "";
        public string ma_kh { get; set; } = "";
        public string ma_nh { get; set; } = "";
        public string ma_kho { get; set; } = "";
        public string ma_vt { get; set; } = "";
        public string ma_imei { get; set; } = "";
        public string ma_cuahang { get; set; } = "";
        public string ma_ca { get; set; } = "";
        public string nh_vt1 { get; set; } = "";
        public string nh_vt2 { get; set; } = "";
        public string nh_vt3 { get; set; } = "";
        public string nh_vt4 { get; set; } = "";
        public string nh_vt5 { get; set; } = "";
        public string ma_nganh { get; set; } = "";

    }
}
