using Genbyte.Sys.Common;

namespace Category.Vdmgia_bl
{
    public class EntityItem
    {

        [IsPrimary]
        public string ma_vt { get; set; }

        public string ten_vt { get; set; }

        [IsPrimary]
        public string ma_nt { get; set; }

        [IsPrimary]
        public DateTime ngay_ban { get; set; }

        [IsPrimary]
        public string ma_cuahang { get; set; }

        public decimal gia_vat_nt { get; set; }

        public decimal gia_nt2 { get; set; }
    }
}
