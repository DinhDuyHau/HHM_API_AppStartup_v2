using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genbyte.Component.Voucher;

namespace Voucher.SVTran_HDF.Models
{
    internal class SoldProductDetail : DetailEntity
    {
        public string ma_vt { get; set; }

        public string ma_imei { get; set; }
    }
}
