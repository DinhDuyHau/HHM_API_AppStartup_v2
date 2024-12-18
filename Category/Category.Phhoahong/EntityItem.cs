using Genbyte.Sys.Common;

namespace Category.Phhoahong
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_hoahong { get; set; }
        public string ten_hoahong { get; set; }
        public string ma_phanloai { get; set; }
        public string ngay_hl { get; set; }
        public string status { get; set; }
    }
}
