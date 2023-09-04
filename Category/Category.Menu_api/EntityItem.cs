using Genbyte.Sys.Common;

namespace Category.Menu_api
{
    public class EntityItem
    {
        [IsPrimary]
        public string wmenu_id { get; set; }

        public string menu_id { get; set; }

        public string wmenu_id0 { get; set; }

        //[IgnoreDbUpdate(true)]
        public string bar { get; set; }

        public string bar2 { get; set; }
        public string icon { get; set; }
        public string icon_url { get; set; }
        public string link { get; set; }
        public string parameter { get; set; }
        public string syscode { get; set; }
        public string sysid { get; set; }
        public string type { get; set; }
        public bool filterMode { get; set; }
    }
}
