using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upload.Models
{
    public class CustomerUpload
    {
        [Required]
        [MaxLength(16)]
        public string ma_kh { get; set; }
        public IFormFile? image { get; set; }
    }
}
