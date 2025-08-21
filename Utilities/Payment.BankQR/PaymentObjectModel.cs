using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.BankQR
{
    public class PaymentObjectModel
    {
        public int error_code { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public object result { get; set; }
    }
}
