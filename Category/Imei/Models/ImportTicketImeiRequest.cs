using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei.Models
{
    public class ImportTicketImeiRequest
    {
        public string ma_ct { get; set; }
        public string so_ct { get; set; }
        public string ma_gd { get; set; }

        public string nh_vt1 { get; set; }
        public string nh_vt2 { get; set; }
        public string nh_vt3 { get; set; }
        public string nh_vt4 { get; set; }
        public string ma_vt { get; set; }

        public string ma_cuahang { get; set; }
    }
}