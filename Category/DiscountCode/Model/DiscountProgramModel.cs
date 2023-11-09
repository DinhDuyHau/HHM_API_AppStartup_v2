using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountCode.Model
{
    public class DiscountProgramModel
    {
        public string ma_ctr { get; set; }
        public string ten_ctr { get; set; }
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public DateTime? ngay_hl { get; set; }
        public DateTime? ngay_hl2 { get; set; }
        public decimal tien_giam { get; set; }
        public decimal tl_giam { get; set; }
    }
}
