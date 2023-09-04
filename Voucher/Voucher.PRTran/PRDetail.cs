using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.PRTran
{
    public class PRDetail : DetailEntity
    {
        public string ma_vt { get; set; }

        public decimal so_luong { get; set; }
    }
}
