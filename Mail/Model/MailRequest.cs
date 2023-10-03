using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mail.Model
{
    public class MailRequest
    {
        public string send_to { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public List<IFormFile>? files { get; set; }
    }
}
