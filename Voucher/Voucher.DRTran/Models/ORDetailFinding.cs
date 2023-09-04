using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.DRTran.Models
{
    public class ORDetailFinding: PRDetail
    {
        public string so_hd_tt { get; set; }
        public DateTime? ngay_hd_tt { get; set; }
        public decimal tien_hd { get; set; }
        public decimal da_tt { get; set; }
        public decimal tien_cl { get; set; }
        public string stt_rec_tt { get; set; }
        public decimal con_lai { get; set; }
    }
}
