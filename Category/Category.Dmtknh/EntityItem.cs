using Genbyte.Sys.Common;

namespace Category.Dmtknh
{
    public class EntityItem
    {
        [IsPrimary]
        public string tk { get; set; }

        public string tknh { get; set; }

        public string ten_nh { get; set; }

        //[IgnoreDbUpdate(true)]
        public string ten_nh2 { get; set; }

        public string tinh_thanh { get; set; }
        public string phone { get; set; }
    }
}
