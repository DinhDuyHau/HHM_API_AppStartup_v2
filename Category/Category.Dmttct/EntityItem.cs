using Genbyte.Sys.Common;

namespace Category.Dmttct
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_ct { get; set; }
        [IsPrimary]
        public string loai_gd { get; set; }
        [IsPrimary]
        public string status { get; set; }

        //[IgnoreDbUpdate(true)]
        public string statusname { get; set; }

        public string statusname2 { get; set; }
        public bool xdefault { get; set; }
        public bool right_yn { get; set; }
        public bool xdel { get; set; }
        public bool xedit { get; set; }

        public int xorder { get; set; }
    }
}
