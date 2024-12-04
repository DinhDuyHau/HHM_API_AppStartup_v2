using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptHcare
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }
        
        public string ma_nvbh { get; set; } = "";

        public string ma_asm{ get; set; } = ""; 

        public string ma_cuahang { get; set; } = "";

        public int mau_bc { get; set; } = 50;

    }
}
