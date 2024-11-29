using Genbyte.Sys.Common;

namespace Category.Dmcuahang
{
    public class EntityItem
    {
        public string ma_dvcs { get; set; }

        public string ma_tinh { get; set; }

        [IsPrimary]
        public string ma_cuahang { get; set; }

        public string ten_cuahang { get; set; }

        public string dia_chi { get; set; }

        public string dien_thoai { get; set; }
        public string tk_nh_chotca { get; set; }

        public string status { get; set; }

        public string ma_kvkd { get; set; }

    }
}
