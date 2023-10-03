using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class KeyServiceModel
    {
        public string ma_key { get; set; }
        public string loai_key { get; set; }
        public string ten_loaikey { get; set; }
        public string ma_dv { get; set; }
        public string ten_dv { get; set; }
        public DateTime ngay_nhap { get; set; }
        public DateTime ngay_het_han { get; set; }
    }
}
