using Genbyte.Sys.Common;

namespace Category.Dmkho
{
    public class EntityItem
    {
        public string ma_dvcs { get; set; }

        [IsPrimary]
        public string ma_kho { get; set; }

        public string ten_kho { get; set; }

        public string ten_kho2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public Boolean dai_ly_yn { get; set; }

        public string ma_nh { get; set; }
        public string ma_loai { get; set; }

        public string ghi_chu { get; set; }
        public string ma_cuahang { get; set; }

        public string status { get; set; }

    }
}
