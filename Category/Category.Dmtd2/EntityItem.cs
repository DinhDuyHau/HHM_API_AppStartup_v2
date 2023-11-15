using Genbyte.Sys.Common;

namespace Category.Dmtd2
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_td { get; set; }

        public string ten_td { get; set; }

        public string ten_td2 { get; set; }

        public bool sd_imei_yn { get; set; }

        //[IgnoreDbUpdate(true)]
    }
}
