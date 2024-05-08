using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei.Models
{
    public class RenewModel
    {
        public string ma_imei { get; set; }
        public string ma_cuahang { get; set; }
        public string ma_ncc { get; set; }
        public List<string> list_vt { get; set; }

        public string imei_thu_cu { get; set; }
    }
}
