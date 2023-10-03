using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class KeyToSendEmail
    {
        public string ma_key { get; set; }
        public string loai_key { get; set; }
        public string ten_loaikey { get; set; }
        public string ma_dv { get; set; }
        public string ten_dv { get; set; }
        public string ma_kh { get; set; }
        public string email0 { get; set; }
        public string email2 { get; set; }
        public DateTime ngay_nhap { get; set; }
        public DateTime ngay_het_han { get; set; }
    }
}
