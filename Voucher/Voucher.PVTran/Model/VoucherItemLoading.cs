using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.PVTran.Model
{
    public class VoucherItemLoading: VoucherItem
    {
        public string ten_kh { get; set; }
        public string ten_tt { get; set; }
        public string ten_gd { get; set; }
        public string ten_cuahang { get; set; }

        //ngày hệ thống
        public DateTime ngay_ht { get; set; }
    }
}
