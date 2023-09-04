using Genbyte.Sys.Common;

namespace Category.Vdmdichvu
{
    public class EntityItem
    {

        [IsPrimary]
        public string ma_dv { get; set; }

        public string ma_dv2 { get; set; }

        public string ten_dv { get; set; }

        public string ten_dv2 { get; set; }

        public string dvt { get; set; }

        public string ma_kho { get; set; }

        //[IgnoreDbUpdate(true)]
        public string loai_vt { get; set; }

        public string status { get; set; }

        public bool vt_ton_kho { get; set; }

        public string ma_thue { get; set; }

        public decimal thue_suat { get; set; }
    }
}
