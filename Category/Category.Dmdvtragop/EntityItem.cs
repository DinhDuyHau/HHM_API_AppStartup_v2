using Genbyte.Sys.Common;

namespace Category.Dmdvtragop
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_dvtg { get; set; }

        public string ten_dvtg { get; set; }

        public string ten_dvtg2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string dia_chi { get; set; }

        public string phone { get; set; }
    }
}
