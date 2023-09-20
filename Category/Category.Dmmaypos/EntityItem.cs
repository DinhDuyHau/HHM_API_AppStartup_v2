using Genbyte.Sys.Common;

namespace Category.Dmmaypos
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_pos { get; set; }

        public string ten_pos { get; set; }

        public string ten_pos2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string ma_cuahang { get; set; }

        public string tk_nganhang { get; set; }
    }
}
