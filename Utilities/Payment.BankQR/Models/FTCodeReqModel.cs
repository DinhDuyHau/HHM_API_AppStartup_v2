using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.BankQR.Models
{
    public class FTCodeReqModel
    {
        public string ma_kh { get; set; }

        public List<Dictionary<string, string>> filter {  get; set; }
    }

}
