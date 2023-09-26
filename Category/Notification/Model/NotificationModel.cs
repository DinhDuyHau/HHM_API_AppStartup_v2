using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Model
{
    public class NotificationModel
    {
        public string notification_id {  get; set; }
        public string send_from {  get; set; }
        public string send_to { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public DateTime send_date { get; set; }
        public string status { get; set; }
        public string redirect { get; set; }
    }
}
