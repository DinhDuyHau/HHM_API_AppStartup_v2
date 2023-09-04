using Genbyte.Sys.Common;

namespace Category.Dmloaithuho
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_loai { get; set; }

        public string ten_loai { get; set; }

        public string ten_loai2 { get; set; }

        //[IgnoreDbUpdate(true)]
    }
}
