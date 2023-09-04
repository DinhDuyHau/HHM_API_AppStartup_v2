using Genbyte.Sys.Common;

namespace Category.Dmbp
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_bp { get; set; }

        public string ten_bp { get; set; }

        public string ten_bp2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string dia_chi { get; set; }

        public string dien_thoai { get; set; }
    }
}
