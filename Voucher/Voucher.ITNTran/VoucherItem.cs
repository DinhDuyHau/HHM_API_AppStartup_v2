using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.ITNTran
{
    public class VoucherItem : VoucherEntity
    {
        public string ma_nk { get; set; }
        public string dien_giai { get; set; }
        public string ma_kh { get; set; }
        public string ma_gd { get; set; }
        public string ma_nt { get; set; }
        public decimal ty_gia { get; set; }
        public string ma_kho { get; set; }
        public string ma_khon { get; set; }
        public string ma_cuahang_n { get; set; }
        public string loai_ct { get; set; }

        public decimal t_so_luong { get; set; }
        public decimal t_tien_nt { get; set; }
        public decimal t_tien { get; set; }
        public string ma_thue { get; set; }
        public decimal thue_suat { get; set; }
        public decimal t_thue { get; set; }
        public decimal t_thue_nt { get; set; }




        public string hddt_ma_ncc { get; set; }
        public string hddt_mau_hd { get; set; }
        public string hddt_so_seri { get; set; }
        public DateTime? hddt_ngay_hd { get; set; }
        public DateTime? hddt_ngay_ky { get; set; }
        public string hddt_so_hd { get; set; }
        public string hddt_status { get; set; }
        public string hddt_ma_so_thue { get; set; }
        public string hddt_ma_tra_cuu { get; set; }



    }
}
