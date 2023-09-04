using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.IPNTran
{
    public class VoucherItem : VoucherEntity
    {
        public string dien_giai { get; set; }
        public string ma_gd { get; set; }
        public string ma_nt { get; set; }
        public decimal ty_gia { get; set; }
        public string ma_kho { get; set; }
        public string ma_khox { get; set; }
        public string ma_cuahang_x { get; set; }
        public string loai_ct { get; set; }

        public decimal t_so_luong { get; set; }
        public decimal t_tien_nt { get; set; }
        public decimal t_tien { get; set; }
        public string so_ct0 { get; set; }
        public string so_seri0 { get; set; }
        public DateTime? ngay_ct0 { get; set; }

        public string so_ct0_xuat { get; set; }
        public string so_seri0_xuat { get; set; }
        public DateTime? ngay_ct0_xuat { get; set; }


    }
}
