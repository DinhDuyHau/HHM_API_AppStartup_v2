using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.SVTran_DV1
{
    public class VoucherItem : VocherItemBase
    {
        public decimal t_ck { get; set; }
        public decimal t_ck_nt { get; set; }
        public decimal t_da_tra { get; set; }
        public decimal t_con_no { get; set; }
        public string ma_nk { get; set; }
        public string so_seri { get; set; }
        public string email_nhan_key { get; set; }
        public string fnote3 { get; set; }
        public string fnote2 { get; set; }
        public string hd_nguoi_mua { get; set; }
        public string hd_loai_giay_to { get; set; }
        public string hd_so_giay_to { get; set; }
        public string xtag { get; set; }
    }
}
