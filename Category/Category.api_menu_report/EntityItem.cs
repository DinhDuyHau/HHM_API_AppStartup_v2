using Genbyte.Sys.Common;

namespace Category.Api_menu_report
{
    public class EntityItem
    {
        [IsPrimary]
        public string sysid { get; set; }
        [IsPrimary]
        public string controller { get; set; }
        [IsPrimary]
        public string form_id { get; set; }

        public string name { get; set; }

        public string name2 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string status { get; set; }
        public string type { get; set; }
    }
}
