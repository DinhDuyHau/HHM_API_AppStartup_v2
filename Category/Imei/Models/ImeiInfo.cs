using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei.Models
{
    public class ImeiInfo: ImeiState
    {
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public string dvt { get; set; }
        public string ma_thue { get; set; }
        public decimal thue_suat { get; set; }
    }
}
