using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.IPNTran
{
    public class IPDetail : DetailEntity
    {
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public string dvt { get; set; }

        public string ma_imei { get; set; }
        public string ma_imei_xuat { get; set; }

        public decimal so_luong { get; set; }

        public decimal gia_nt { get; set; }

        public decimal tien_nt { get; set; }
        public string tk_vt { get; set; }
        public string tk_du { get; set; }
        public string ma_nx { get; set; }
    }
}
