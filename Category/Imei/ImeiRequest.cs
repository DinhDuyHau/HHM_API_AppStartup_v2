using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei
{
    public class ImeiRequest 
    { 
        public string ma_imei { get; set; } 
        public string ma_cuahang { get; set; } 
        public string ma_ct { get; set; } 
        public string? ma_kh { get; set; } 
        public DateTime? ngay_ct { get; set; } 
    }
}
