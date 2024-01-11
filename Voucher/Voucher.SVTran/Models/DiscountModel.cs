using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.SVTran.Models
{
    public class DiscountModel
    {
        public string ma_ck { get; set; }
        public string ten_ck { get; set; }
        public DateTime ngay_bd { get; set; }
        public DateTime ngay_kt { get; set; }
        public string gio_bd { get; set; }
        public string gio_kt { get; set; }
        public string loai_ck { get; set; }
        public string ten_loai { get; set; }
        public int type { get; set; }
        public decimal tien_ck { get; set; }
        public decimal tl_ck { get; set; }
        public decimal tien_ck_max { get; set; }
        public decimal tien_ck_tl { get; set; }
        public decimal tien_ck_ct { get; set; }
        public int uu_tien { get; set; }
        public decimal sl_nhom { get; set; }
        public object items { get; set; }
    }
}
