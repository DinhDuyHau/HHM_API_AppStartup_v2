using Genbyte.Sys.Common;

namespace Category.Dmck2
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_ck { get; set; }

        public string ten_ck { get; set; }

        public DateTime? ngay_bd { get; set; }
        public DateTime? ngay_kt { get; set; }
        public string gio_bd { get; set; }
        public string gio_kt { get; set; }
        public string loai_ck { get; set; }
        public string ma_dvcs { get; set; }
        public string ma_cuahang { get; set; }
        public string ma_kho { get; set; }
    }
}
