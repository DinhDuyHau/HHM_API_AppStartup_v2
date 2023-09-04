using Genbyte.Sys.Common;

namespace Category.Vdmctmoban
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_ctr { get; set; }
        public string ten_ctr { get; set; }
        [IsPrimary]
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public DateTime? ngay_hl { get; set; }
        public DateTime? ngay_tra { get; set; }
        public decimal tien_coc { get; set; }

    }
}
