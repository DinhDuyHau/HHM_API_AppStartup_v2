using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upload.Models
{
    public class WholeSaleUpload2
    {
        public string stt_rec { get; set; }
        public DateTime ngay_ct { get; set; }
        public IFormFile? co_file {  get; set; }
        public IFormFile? cq_file {  get; set; }
    }
}
