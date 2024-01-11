using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.PVTran
{
    public class VoucherItem : VoucherEntity
    {
        public DateTime? ngay_lct { get; set; }
        public string ma_nt { get; set; }
        public string dien_giai { get; set; }

        public string loai_ct { get; set; }
        public string ma_kh { get; set; }
        public string ma_tt { get; set; }
        public string ma_gd { get; set; }

        public decimal t_so_luong { get; set; }
        public decimal t_tien_nt { get; set; }
        public decimal t_thue_nt { get; set; }
        public decimal t_tt_nt { get; set; }

        public DateTime? ngay_ct0 { get; set; }
        public string so_ct0 { get; set; }
        public string so_seri0 { get; set; }
    }
}
