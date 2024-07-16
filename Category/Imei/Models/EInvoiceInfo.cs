using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imei.Models
{
    public class EInvoiceInfo
    {
        public string stt_rec { get; set; }

        public DateTime? ngay_ct { get; set; }

        public string ma_ct { get; set; }

        public string ma_kh { get; set; }

        public string ma_so_thue { get; set; }

        public string mau_hoa_don { get; set; }

        public string so_seri { get; set; }

        public string so_hoa_don { get; set; }

        public string ghi_chu { get; set; }

        public string ma_ncc { get; set; }

        public string id_giaodich { get; set; }

        public string ma_bi_mat { get; set; }

        public DateTime? ngay_ky { get; set; }

        public int lan_dieu_chinh { get; set; }

        public string status { get; set; }
    }
}
