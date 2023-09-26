using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.PVTran2
{
    public class TaxDetail : DetailEntity
    {
        public string ma_dvcs { get; set; } 
	    public string loai_ct { get; set; } 
	    public DateTime? ngay_lct { get; set; }

	    public DateTime? ngay_ct0 { get; set; }

        public string so_ct0 { get; set; }
	    public string so_seri0 { get; set; }
	    public string mau_bc { get; set; } 
	    public string ma_tc { get; set; } 
	    public string ma_kh { get; set; } 
	    public string ten_kh { get; set; }

        public string dia_chi { get; set; }

        public string ma_so_thue { get; set; }
	    public string ma_kh2 { get; set; } 
	    public string ten_vt { get; set; }

        public decimal so_luong { get; set; }

        public decimal ty_gia { get; set; }

        public string ma_nt { get; set; } 
	    public decimal gia_nt { get; set; }

        public decimal gia { get; set; }

        public decimal t_tien_nt { get; set; }

        public decimal t_tien { get; set; }

        public string ma_thue { get; set; } 
	    public decimal thue_suat { get; set; }

        public decimal t_thue_nt { get; set; }

        public decimal t_thue { get; set; }

        public string ma_tt { get; set; } 
	    public string tk_thue_no { get; set; } 
	    public string tk_du { get; set; } 
	    public string ma_kho { get; set; } 
	    public string ma_vv { get; set; } 
	    public string ma_sp { get; set; } 
	    public string ma_bp { get; set; } 
	    public string so_lsx { get; set; } 
	    public string ghi_chu { get; set; }

        public decimal nam { get; set; }

        public decimal ky { get; set; }

        public string status { get; set; } 
	    public DateTime? datetime0 { get; set; }

        public DateTime? datetime2 { get; set; }

        public int user_id0 { get; set; }
        public int user_id2 { get; set; }
        public string ma_hd { get; set; } 
	    public string ma_ku { get; set; } 
	    public string ma_phi { get; set; } 
	    public string so_dh { get; set; } 
	    public string ma_td1 { get; set; } 
	    public string ma_td2 { get; set; } 
	    public string ma_td3 { get; set; } 
	    public decimal sl_td1 { get; set; }

        public decimal sl_td2 { get; set; }

        public decimal sl_td3 { get; set; }

        public DateTime? ngay_td1 { get; set; }

        public DateTime? ngay_td2 { get; set; }

        public DateTime? ngay_td3 { get; set; }

        public string gc_td1 { get; set; }

        public string gc_td2 { get; set; }

        public string gc_td3 { get; set; }

        public string s1 { get; set; } 
	    public decimal s4 { get; set; }

        public decimal s5 { get; set; }

        public decimal s6 { get; set; }

        public DateTime? s7 { get; set; }

        public DateTime? s8 { get; set; }

        public DateTime? s9 { get; set; }

        public string ma_mau_ct { get; set; } 
    }
}
