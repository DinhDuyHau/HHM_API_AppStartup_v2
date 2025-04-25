using Genbyte.Component.Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.RptBankingBK
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }

        public DateTime den_ngay { get; set; }

        public int ngan_hang { get; set; } = 1;

        public string ma_cuahang { get; set; } = "";

        public string ds_ma_gd { get; set; } = "";


    }
}
