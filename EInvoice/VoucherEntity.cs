using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EInvoice
{
    public class VoucherEntity
    {
        public string stt_rec { get; set; }

        public string ma_ct { get; set; }

        public string so_ct { get; set; }

        public DateTime? ngay_ct { get; set; }

        public string ma_dvcs { get; set; }

        public string ma_cuahang { get; set; }
        public string ma_kh { get; set; }

        public string ma_ca { get; set; }

        public string status { get; set; }
        public string ma_nk { get; set; }
        public string dien_giai { get; set; }
        public decimal t_tien { get; set; }
        public decimal t_tien_nt { get; set; }
        public string ma_thue { get; set; }
        public decimal thue_suat { get; set; }
        public decimal t_thue { get; set; }
        public decimal t_thue_nt { get; set; }
        public string ma_nt { get; set; }

        public string hd_mst { get; set; }

        public string hd_ten_kh { get; set; }

        public string hd_dia_chi { get; set; }

        public string hd_httt { get; set; }

        public string hd_email { get; set; }

        public IList<VoucherDetail> details { get; set; }
    }
}
