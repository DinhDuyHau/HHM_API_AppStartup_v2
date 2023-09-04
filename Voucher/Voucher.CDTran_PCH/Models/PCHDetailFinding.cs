using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.CDTran_PCH.Models
{
    public class PCHDetailFinding: PRDetail
    {
        public string so_hd_tt { get; set; }
        public DateTime? ngay_hd_tt { get; set; }
        public decimal tien_hd { get; set; }
        public decimal da_tt { get; set; }
        public decimal tien_cl { get; set; }
        public string stt_rec_tt { get; set; }
        public decimal con_lai { get; set; }

        public decimal da_tt_nt { get; set; }
        public decimal t_tt_nt { get;set; }
        public decimal cl_nt { get;set; }


    }
}
