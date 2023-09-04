using Genbyte.Sys.Common;

namespace Category.Dmphuongxa
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_phuong { get; set; }

        public string ten_phuong { get; set; }
        public string ten_phuong2 { get; set; }

        //[IgnoreDbUpdate(true)]
    }
}
