using Genbyte.Sys.Common;

namespace Category.Dmtt
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_tt { get; set; }

        public string ten_tt { get; set; }

        public string ten_tt2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string status { get; set; }

        public decimal han_tt { get; set; } 

        public string ten_ngan { get; set; }

        public string ten_ngan2 { get; set; }
    }
}
