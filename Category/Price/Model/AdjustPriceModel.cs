using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Price.Model
{
    public class AdjustPriceModel
    {
        public string ma_vt_mua { get; set; }

        public string ma_vt_ban { get; set; }

        public decimal tien_dc_max { get; set; }

        public decimal tien_dc_min { get; set; }

        public decimal tien_ht { get; set; }
    }
}
