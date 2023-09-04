using Genbyte.Sys.Common;

namespace Category.Dmmagd
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_ct { get; set; }
        [IsPrimary]
        public string loai_ct { get; set; }
        [IsPrimary]
        public string ma_gd { get; set; }

        public string ten_gd { get; set; }
    }
}
