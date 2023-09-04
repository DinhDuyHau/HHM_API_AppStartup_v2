using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptStockSummary
{
    public class ParamItem
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_cuahang { get; set; } = "";
        public string ma_kho { get; set; } = "";
        public string ma_vt { get; set; } = "";
        public string ma_dvcs { get; set; } = "";
        public string loai_vt { get; set; } = "";
        public string tt_sx1 { get; set; } = "0";
        public string tt_sx2 { get; set; } = "0";
        public string tt_sx3 { get; set; } = "0";

        public string nh_vt1 { get; set; } = "";
        public string nh_vt2 { get; set; } = "";
        public string nh_vt3 { get; set; } = "";
        public string tinh_ps { get; set; } = "";
        public string group { get; set; } = "";
        public string order { get; set; } = "";
        public string showItem { get; set; } = "";
        public int dataType { get; set; } = 0;
        public string language { get; set; } = "V";
        public int userId { get; set; } = 1;
        public bool admin { get; set; } = true;
        public string in_sl { get; set; } = "1";
        public int page_index { get; set; } = 1;
        public int page_size { get; set; } = 0;
    }
}
