using Genbyte.Component.Voucher;

namespace Service.ModelDV1
{
    public class VoucherItemDV1 : VocherItemBase
    {
        public decimal t_ck { get; set; }
        public decimal t_da_tra { get; set; }
        public decimal t_con_no { get; set; }
        public string ten_kh { get; set; }
        public List<SVDetailDV1> items { get; set; }
    }
}
