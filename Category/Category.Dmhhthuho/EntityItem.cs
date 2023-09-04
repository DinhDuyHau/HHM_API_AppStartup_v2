using Genbyte.Sys.Common;

namespace Category.Dmhhthuho
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_td { get; set; }
        [IsPrimary]
        public DateTime ngay_hl { get; set; }
        public decimal ti_le { get; set; }
        public decimal tien { get; set; }
    }
}
