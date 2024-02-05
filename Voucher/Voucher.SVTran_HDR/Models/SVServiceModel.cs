using Genbyte.Component.Voucher;

namespace Voucher.SVTran_HDR
{
    public class SVServiceModel : ServiceDetailBase
    {
        public bool giam_gia_yn { get; set; }
        public decimal ty_le_giam { get; set; }
        public decimal tien_giam { get; set; }
        public string ma_asm_duyet { get; set; }
        public string ten_asm_duyet { get; set; }
        public decimal gia_tra_lai { get; set; }
    }
}
