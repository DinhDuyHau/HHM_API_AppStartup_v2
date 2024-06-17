using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei.Models
{
    public class ImeiWarrantyOut
    {
        public string ma_imei { get; set; }

        public string so_ct_px { get; set; }

        public string ma_vt { get; set; }   

        public string ten_vt { get; set; }
        
        public string dvt { get; set; }

        public decimal gia { get; set; }
    }
}
