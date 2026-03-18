using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei.Models
{
    public class ImportTicketImei
    {
        public string ma_imei { get; set; }
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public string dvt { get; set; }
        public string ma_kho { get; set; }
        public string stt_rec { get; set; }
        public string stt_rec0 { get; set; }

        public DateTime ngay_ct { get; set; }
        public string so_ct { get; set; }
    }
}