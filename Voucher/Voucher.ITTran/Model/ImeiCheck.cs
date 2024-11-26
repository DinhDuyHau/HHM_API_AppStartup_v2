using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.ITTran.Model
{
    internal class ImeiCheck
    {
        public string ma_imei { get; set; }
        public string ma_kho { get; set; }
        public bool exists_yn { get; set; }
    }
}
