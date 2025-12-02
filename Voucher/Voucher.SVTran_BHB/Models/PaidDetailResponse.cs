using Genbyte.Component.Voucher;

namespace Voucher.SVTran_BHB.Models
{
    public class PaidDetailResponse : PaidDetailBaseResponse
    {
        public string gc_td1 { get; set; }
        public string gc_td2 { get; set; }
        public string gc_td3 { get; set; }
    }
}
