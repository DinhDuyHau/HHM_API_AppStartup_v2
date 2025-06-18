using Genbyte.Component.Voucher;

namespace Voucher.SVTran_BHK
{
    public class VoucherItem : VocherItemBase
    {
        public decimal t_ck { get; set; }
        public decimal t_ck_nt { get; set; }
        public decimal t_da_tra { get; set; }
        public decimal t_con_no { get; set; }
        public decimal tien_dat_coc { get; set; }
        public string ma_nvvc { get; set; }
        public decimal t_sl_thu_cu { get; set; }
        public decimal t_tien_thu_cu { get; set; }
        public decimal t_tien_thu_cu_nt { get; set; }
        public string ma_nk { get; set; }
        public string so_seri { get; set; }
        public string email_nhan_key { get; set; }
        public string ma_ncc { get; set; }

        //Lưu tổng tiền hàng bán mới sau VAT
        public decimal s4 { get; set; }
        public string ma_hang { get; set; }
        public decimal tl_tich_diem { get; set; }
        public string fnote3 { get; set; }
        public string fnote2 { get; set; }
        public string hd_nguoi_mua { get; set; }
        public string hd_loai_giay_to { get; set; }
        public string hd_so_giay_to { get; set; }
    }
}
