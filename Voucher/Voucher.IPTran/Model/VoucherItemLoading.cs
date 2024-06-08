using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.IPTran.Model
{
    public class VoucherItemLoading: VoucherItem
    {
        public string ten_kho { get; set; }
        public string ten_khox { get; set; }
        public string ten_cuahang { get; set; }
        public string ten_cuahang_x { get; set; }

        public DateTime? ngay_lct { get; set; }

        public DateTime ngay_ht { get; set; }
    }
}
