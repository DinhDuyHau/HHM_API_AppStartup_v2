using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptSuppliesOnTheRoadReport
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public string tu_so { get; set; } = "";

        public string den_so { get; set; } = "";

        public string ma_cuahang_x { get; set; } = "";

        public string ma_cuahang_n { get; set; } = "";

        public string ma_kho_x { get; set; } = "";

        public string ma_kho_n { get; set; } = "";

        public string ma_ca { get; set; } = "";

        public string ma_vt { get; set; } = "";

        public string nh_vt1 { get; set; } = "";

        public string nh_vt2 { get; set; } = "";

        public string nh_vt3{ get; set; } = "";

        public string nh_vt4 { get; set; } = "";

        public string nh_vt5 { get; set; } = "";

        public string nh_vt6 { get; set; } = "";

        public string ma_nganh { get; set; } = "";

        public string loai { get; set; } = "1";

    }
}
