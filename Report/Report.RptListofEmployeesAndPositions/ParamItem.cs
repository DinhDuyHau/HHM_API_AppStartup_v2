using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptListofEmployeesAndPositions
{
    public class ParamItem
    {
        public string ma_nvbh { get; set; } = "";
        public string ma_bp { get; set; } = "";
        public string ma_cuahang { get; set; } = "";
        public string language { get; set; } = "V";
        public int userId { get; set; } = 1;
        public bool admin { get; set; } = true;
        public int page_index { get; set; } = 1;
        public int page_size { get; set; } = 0;
    }
}
