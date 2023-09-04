using Genbyte.Sys.Common;

namespace Category.Dmvt
{
    public class EntityItem
    {

        [IsPrimary]
        public string ma_vt { get; set; }

        public string ma_vt2 { get; set; }

        public string ten_vt { get; set; }

        public string ten_vt2 { get; set; }

        public string dvt { get; set; }

        public string ma_kho { get; set; }

        //[IgnoreDbUpdate(true)]
        public int gia_ton { get; set; }

        public string loai_vt { get; set; }

        public string tk_vt { get; set; }

        public string tk_dt { get; set; }

        public string tk_gv { get; set; }

        public string tk_spdd { get; set; }

        public string nh_vt1 { get; set; }

        public string nh_vt2 { get; set; }

        public string nh_vt3 { get; set; }

        public string status { get; set; }

    }
}
