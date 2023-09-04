using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EInvoice
{
    public class ResponseInfo
    {
        public string voucherId { get; set; }
        public string refVoucherId { get; set; }
        public string errorCode { get; set; }
        public string description { get; set; }
        public Result result { get; set; }
        public string fileName { get; set; }
        public byte[] fileToBytes { get; set; }

        public class Result
        {
            public string supplierTaxCode { get; set; }
            public string invoiceNo { get; set; }
            public string transactionID { get; set; }
            public string reservationCode { get; set; }

            public string buyerTaxCode { get; set; }

            public string invoiceForm { get; set; }

            public string invoiceSerial { get; set; }

            public DateTime? invoiceDate { get; set; }

            public DateTime? signedDate { get; set; }

            public string status_v { get; set; }

            public string status_e { get; set; }

        }
    }
}
