using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Price.Model
{
    public class RenewPriceModel
    {
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public string ma_loai { get; set; }
        public string ten_loai { get; set; }
        public string ma_kh { get; set; }
        public string ten_kh { get; set; }
        public decimal gia { get; set; }
        public decimal gia_nt { get; set; }
        public string ma_nt { get; set; }
    }
}
