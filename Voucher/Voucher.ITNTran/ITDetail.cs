using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.ITNTran
{
    public class ITDetail : DetailEntity
    {
        public string stt_rec { get; set; }

        public string stt_rec0 { get; set; }

        public string ma_ct { get; set; }

        public DateTime? ngay_ct { get; set; }

        public string so_ct { get; set; }

        public string ma_cuahang { get; set; }

        public string ma_ca { get; set; }

        public decimal line_nbr { get; set; }
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public string dvt { get; set; }
        public string ma_kho { get; set; }
        public string ma_khon { get; set; }

        public string ma_imei { get; set; }

        public decimal so_luong { get; set; }

        public decimal gia_nt { get; set; }

        public decimal tien_nt { get; set; }
        public string tk_vt { get; set; }
        public string tk_du { get; set; }
        public string ma_nx { get; set; }
        public bool px_gia_dd { get; set; }
        public string stt_rec_yc { get; set; }
        public string stt_rec0yc { get; set; }

    }
}
