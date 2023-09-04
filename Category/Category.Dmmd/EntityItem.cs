using Genbyte.Sys.Common;

namespace Category.Dmmd
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_md { get; set; }
        [IsPrimary]
        public string ma_ct { get; set; }

        public string ten_md { get; set; }
    }
}
