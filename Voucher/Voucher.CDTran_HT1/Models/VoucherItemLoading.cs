using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.CDTran_HT1.Models
{
    public class VoucherItemLoading: VoucherItem
    {
        public string ten_kh { get; set; }

        public decimal tien_da_hoan { get; set; }

        public decimal tien_con_lai { get; set; }

        public string ten_ct_tt { get; set; }
    }
}
