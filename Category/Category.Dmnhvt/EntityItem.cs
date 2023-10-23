using Genbyte.Sys.Common;

namespace Category.Dmnhvt
{
    public class EntityItem
    {
        [IsPrimary]
        public string loai_nh { get; set; }
        [IsPrimary]
        public string ma_nh { get; set; }

        public string ten_nh { get; set; }


    }
}
