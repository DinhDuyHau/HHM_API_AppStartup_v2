using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptSalespersonCommission
{
    public class ParamItem
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_kh { get; set; } = "";
        public string language { get; set; } = "V";
        public int userId { get; set; } = 1;
        public bool admin { get; set; } = true;
        public int page_index { get; set; } = 1;
        public int page_size { get; set; } = 0;
    }
}
