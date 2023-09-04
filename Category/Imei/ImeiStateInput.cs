using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei
{
    public class ImeiStateInput
    {
        public List<string> imeis { get; set; }

        public string? ma_ct { get; set; }
        public bool state { get; set; }

        public int? nxt { get; set; }
    }
}
