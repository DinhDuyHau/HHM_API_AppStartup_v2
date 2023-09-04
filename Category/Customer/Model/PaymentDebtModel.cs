using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer.Model
{
    public class PaymentDebtModel
    {
        public string stt_rec { get; set; }
        public string so_ct { get; set; }
        public DateTime? ngay_ct { get; set; }
        public decimal t_tt_nt { get; set; }
        public decimal da_tt_nt { get; set; }
        public decimal cl_nt { get; set; }
        public string ma_nt { get; set; }
        public decimal tt_nt { get; set; }
        public string dien_giai { get; set; }
    }
}
