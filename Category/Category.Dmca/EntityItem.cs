using Genbyte.Sys.Common;

namespace Category.Dmca
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_ca { get; set; }

        public string ten_ca { get; set; }

        public string ten_ca2 { get; set; }

        public decimal tu_gio { get; set; }

        public decimal den_gio { get; set; }

    }
}
