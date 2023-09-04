using Genbyte.Sys.Common;

namespace Category.Dmdvcs
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_dvcs { get; set; } 

        public string ten_dvcs { get; set; }

        public string dia_chi { get; set; }

        //[IgnoreDbUpdate(true)]
        public string status { get; set; }
        public string type { get; set; }
    }
}
