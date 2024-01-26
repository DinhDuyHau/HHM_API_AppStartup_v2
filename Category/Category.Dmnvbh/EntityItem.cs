using Genbyte.Sys.Common;

namespace Category.Dmnvbh
{
    public class EntityItem
    {
        [IsPrimary]
        public string ma_nvbh { get; set; }

        public string ten_nvbh { get; set; }

        public string ten_nvbh2 { get; set; }

        public string m_username { get; set; }
        public string dia_chi { get; set; }
        //[IgnoreDbUpdate(true)]

    }
}
