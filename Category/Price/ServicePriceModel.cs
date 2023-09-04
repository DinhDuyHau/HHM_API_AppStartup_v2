using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Price
{
    public class ServicePriceModel
    {
        public string ma_dv { get; set; }

        public string ma_cuahang { get; set; }

        public DateTime ngay_hl { get; set; }

        public decimal gia_ban { get; set; }
        public string ma_thue { get; set; }
        public decimal thue_suat { get; set; }
    }
}
