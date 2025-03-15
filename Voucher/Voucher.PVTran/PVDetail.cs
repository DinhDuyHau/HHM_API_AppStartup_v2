using Genbyte.Component.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.PVTran
{
    public class PVDetail : DetailEntity
    {
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public string dvt { get; set; }

        public string ma_imei { get; set; }

        public string ma_kho { get; set; }

        public decimal so_luong { get; set; }

        public decimal gia_nt { get; set; }

        public decimal tien_nt { get; set; }
        public string ma_thue { get; set; }

        public decimal thue_suat { get; set; }
        public decimal thue_nt { get; set; }
        public decimal tt_nt { get; set; }
        public string ma_td1 { get; set; }
        public bool budslive { get; set; }
        public decimal he_so { get; set; }
        public decimal gia { get; set; }
        public decimal gia_nt0 { get; set; }
        public decimal gia0 { get; set; }
        public decimal tien { get; set; }
        public decimal thue { get; set; }
        public decimal tt { get; set; }
        public decimal tien0 { get; set; }
        public decimal tien_nt0 { get; set; }
        public string so_dh_i { get; set; }
        public decimal ck { get; set; }
        public decimal ck_nt { get; set; }
        public decimal thue_ck { get; set; }
        public decimal thue_ck_nt { get; set; }
    }
}
