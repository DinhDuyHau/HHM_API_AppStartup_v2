using Genbyte.Sys.Common;

namespace Category.Resources
{
    public class EntityItem
    {
        [IsPrimary]
        public string name { get; set; }
        public string message { get; set; }
        public string message2 { get; set; }
    }
}
