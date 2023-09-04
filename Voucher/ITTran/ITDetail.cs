using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.ITTran
{
    public class ITDetail : DetailEntity
    {
        public string ma_vt { get; set; }

        public string ma_imei { get; set; }

        public decimal so_luong { get; set; }

        public decimal gia_nt2 { get; set; }

        public decimal tien_nt2 { get; set; }
    }
}
