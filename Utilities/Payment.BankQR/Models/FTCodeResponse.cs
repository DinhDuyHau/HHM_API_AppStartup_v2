using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.BankQR.Models
{
    public class FTCodeResponse
    {
        public string ma_ft {  get; set; }

        public string so_ct { get; set; }

        public string ma_ct { get; set; }

        public string ten_ct { get; set; }

        public DateTime? ngay_ct { get; set; }

        public decimal t_tien_tt { get; set; }

        public decimal t_tien_hoan { get; set; }

        public decimal tien_con_lai { get; set; }
    }
}
