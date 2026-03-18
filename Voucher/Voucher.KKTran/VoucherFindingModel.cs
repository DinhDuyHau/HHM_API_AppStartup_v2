using Genbyte.Sys.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.KKTran
{
    public class VoucherFindingModel
    {
        public string stt_rec { get; set; }

        public string ma_ct { get; set; }

        public string so_ct { get; set; }

        public DateTime? ngay_ct { get; set; }

        public string ma_dvcs { get; set; }

        public string ma_cuahang { get; set; }

        public string ma_ca { get; set; }

        public string status { get; set; }

        public string statusname { get; set; }

        public int user_id0 { get; set; }

        public int user_id2 { get; set; }

        public DateTime datetime0 { get; set; }

        public DateTime datetime2 { get; set; }

        public string comment { get; set; }

        public string dien_giai { get; set; }

        public string ma_gd { get; set; }
    }
}
