using Genbyte.Sys.Common;

namespace Category.Dmquanhuyen
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_quan { get; set; }

        public string ten_quan { get; set; }
        public string ten_quan2 { get; set; }

        //[IgnoreDbUpdate(true)]
    }
}
