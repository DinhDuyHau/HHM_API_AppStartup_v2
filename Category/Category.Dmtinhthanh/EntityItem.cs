using Genbyte.Sys.Common;

namespace Category.Dmtinhthanh
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_tinh { get; set; }

        public string ten_tinh { get; set; }
        public string ten_tinh2 { get; set; }

        //[IgnoreDbUpdate(true)]
    }
}
