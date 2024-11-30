using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.SVTran_BHD.Models
{
    public class MobiInvDetail
    {
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public string goodCode {  get; set; }

        /// <summary>
        /// Tên mặt hàng
        /// </summary>
        public string goodName { get; set; }

        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// Chiết khâu trên mặt hàng
        /// </summary>
        public decimal discount { get; set; }

        /// <summary>
        /// Imei/Serial mặt hàng
        /// </summary>
        public string imei { get; set; }

        /// <summary>
        /// Loại mặt hàng (0: Hàng bán; 1: Hàng đính kèm; 2: Hàng khuyến mại; 3: Hàng biếu tặng)
        /// </summary>
        public string attachStatus { get; set; }

        /// <summary>
        /// Đơn giá nhập
        /// </summary>
        public decimal entryPrice { get; set; }
    }
}
