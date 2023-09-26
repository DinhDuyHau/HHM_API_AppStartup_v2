using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Model
{
    public class NotificationRequest
    {
        public string send_from { get; set; }
        public List<string> send_to { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string? imageUrl { get; set; }
        public Dictionary<string, string>? data { get; set; }
    }
}
