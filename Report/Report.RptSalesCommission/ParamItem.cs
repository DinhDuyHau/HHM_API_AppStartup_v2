using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptSalesCommission
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_cuahang { get; set; } = "";
        public string ma_ca { get; set; } = "";
        public string ma_nvbh { get; set; } = "";
        public string nh_vt1 { get; set; } = "";
        public string nh_vt2 { get; set; } = "";
        public string nh_vt3 { get; set; } = "";
        public string nh_vt4 { get; set; } = "";
        public string nh_vt5 { get; set; } = "";
        public string ma_nganh { get; set; } = "";

    }
} 
