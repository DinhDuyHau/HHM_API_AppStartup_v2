using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptListofEmployeesAndPositions
{
    public class ParamItem : ParamItemBase
    {
        public string ma_nvbh { get; set; } = "";
        public string ma_bp { get; set; } = "";
        public string ma_cuahang { get; set; } = "";
    }
}
