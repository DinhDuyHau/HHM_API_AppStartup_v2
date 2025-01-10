using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventGiveAway.Model
{
    public class EventGiveAwayModel
    {
        public string ma_sukien { get; set; }

        public string ten_sukien { get; set; }

        public DateTime? ngay_bd { get; set; }
        public DateTime? ngay_kt { get; set; }
        public string gio_bd { get; set; }
        public string gio_kt { get; set; }
        public string ma_dvcs { get; set; }
        public string ma_cuahang { get; set; }
        public string ghi_chu { get; set; }
        public bool duyet_yn { get; set; }
    }
}
