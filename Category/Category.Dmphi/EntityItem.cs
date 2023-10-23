using Genbyte.Sys.Common;

namespace Category.Dmphi
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_phi { get; set; }

        public string ten_phi { get; set; }

        public string ten_phi2 { get; set; }
        public string tk_cp { get; set; }

        //[IgnoreDbUpdate(true)]

    }
}
