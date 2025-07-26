using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptCrossSellingRate
{
    public class ParamItem : ParamItemBase
    {

        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string ma_nvbh { get; set; } = "";

        public string ma_asm { get; set; } = "";

        public string ma_cuahang { get; set; } = "";

        public string mau_bc { get; set; } = "10";

        public string nh_vt1 { get; set; } = "";

        public string nh_vt2 { get; set; } = "";

        public string nh_vt3 { get; set; } = "";

        public string ma_nganh { get; set; } = "";

        public string nh_vt1_bk { get; set; } = "";

        public string nh_vt2_bk { get; set; } = "";

        public string nh_vt3_bk { get; set; } = "";

        public string ma_nganh_bk { get; set; } = "";
    }
}
