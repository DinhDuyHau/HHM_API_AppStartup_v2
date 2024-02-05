using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.SVTran_BHW
{
    public class VoucherItem : VocherItemBase
    {
        public decimal t_ck { get; set; }
        public decimal t_ck_nt { get; set; }
        public decimal t_da_tra { get; set; }
        public decimal t_con_no { get; set; }
        public decimal tien_dat_coc { get; set; }
        public string ma_nvvc { get; set; }
        public string ma_nk { get; set; }
        public string so_seri { get; set; }
        public string email_nhan_key { get; set; }
        public string nguoi_lap { get; set; }
    }
}
