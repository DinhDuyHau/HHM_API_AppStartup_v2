using Genbyte.Sys.Common;

namespace Category.Dmgia_bl
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_vt { get; set; }
        [IsPrimary]
        public string ma_nt { get; set; }
        [IsPrimary]
        public DateTime ngay_ban { get; set; }

        //[IgnoreDbUpdate(true)]
        public decimal gia_nt2 { get; set; }
    }
}
