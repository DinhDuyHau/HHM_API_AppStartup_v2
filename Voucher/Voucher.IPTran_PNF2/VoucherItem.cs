using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.IPTran_PNF2
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

        /// <summary>
        /// Mã loại giao dịch
        /// </summary>
        public string fnote2 { get; set; }
    }
}
