using Genbyte.Sys.Common;

namespace Category.Dmthue
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_thue { get; set; }

        public string ten_thue { get; set; }

        public decimal thue_suat { get; set; }

        //[IgnoreDbUpdate(true)]
    }
}
