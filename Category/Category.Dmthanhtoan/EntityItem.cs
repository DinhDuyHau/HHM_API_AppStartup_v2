using Genbyte.Sys.Common;

namespace Category.Dmthanhtoan
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_thanhtoan { get; set; }

        public string ten_thanhtoan { get; set; }

        public string ten_thanhtoan2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string status { get; set; }
        public bool isHide { get; set; }
    }
}
