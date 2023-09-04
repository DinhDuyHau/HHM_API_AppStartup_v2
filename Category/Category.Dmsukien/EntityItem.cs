using Genbyte.Sys.Common;

namespace Category.Dmsukien
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_sukien { get; set; }

        public string ten_sukien { get; set; }

        public DateTime? ngay_bd { get; set; }
        public DateTime? ngay_kt { get; set; }
        public string gio_bd { get; set; }
        public string gio_kt { get; set; }
        public string ma_dvcs { get; set; }
        public string ma_cuahang { get; set; }
        public string ghi_chu { get; set; }
    }
}
