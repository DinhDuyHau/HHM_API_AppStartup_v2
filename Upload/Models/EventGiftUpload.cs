using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upload.Models
{
    public class EventGiftUpload
    {
        public string so_ct { get; set; }
        public DateTime ngay_ct { get; set; }
        public IFormFile? image {  get; set; }
    }
}
