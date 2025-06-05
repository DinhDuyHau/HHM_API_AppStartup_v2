using Genbyte.Component.Voucher;

namespace Voucher.SVTran_HDF
{
    public class SVServiceModel : ServiceDetailBase
    {
        public bool giam_gia_yn { get; set; }
        public decimal ty_le_giam { get; set; }
        public decimal tien_giam { get; set; }
        public string ma_asm_duyet { get; set; }
        public string ten_asm_duyet { get; set; }
        public decimal gia_tra_lai { get; set; }
        public string stt_rec_px { get; set; }
        public string stt_rec0px { get; set; }
    }
}
