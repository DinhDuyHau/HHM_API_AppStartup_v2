using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei
{
    public class ImeiState
    {
        public string ma_imei { get;set; }

        public bool exists_yn { get;set; }

        public bool in_store_yn { get; set; }

        public bool xuat_yn { get; set; }

        public bool dat_hang_yn { get; set; }

        public bool dieu_chuyen_yn { get; set; }

        public bool bao_hanh_yn { get; set; }
        public bool ban_hang_yn { get; set; }
        public bool tra_ncc_yn { get; set; }

        public bool in_stock_yn { get; set; }

        public bool ban_lk_yn { get; set; }

    }
}
