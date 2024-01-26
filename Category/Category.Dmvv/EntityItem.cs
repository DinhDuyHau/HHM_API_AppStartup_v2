using Genbyte.Sys.Common;

namespace Category.Dmvv
{
    public class EntityItem
    {

        [IsPrimary]
        public string ma_vv { get; set; }

        public string ten_vv { get; set; }

        public string ten_vv2 { get; set; }

        public string ma_kh { get; set; }

        public string ma_nvbh { get; set; }

        //[IgnoreDbUpdate(true)]
        public DateTime ngay_vv { get; set; }
        public DateTime ngay_vv1 { get; set; }
        public DateTime ngay_vv2 { get; set; }

        public string status { get; set; }

    }
}
