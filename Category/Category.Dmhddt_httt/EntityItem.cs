using Genbyte.Sys.Common;

namespace Category.Dmhddt_httt
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_httt { get; set; }

        public string ten_httt { get; set; }

        public string ten_httt2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public byte xuat_v { get; set; }

        public byte nhap_v { get; set; }
    }
}
