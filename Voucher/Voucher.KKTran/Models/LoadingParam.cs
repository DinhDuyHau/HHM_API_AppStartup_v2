using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.KKTran.Models
{
    public class LoadingParam
    {
        public string order_by { get; set; }

        public int row_number { get; set; }

        public string ext_filter { get; set; }
    }
}
