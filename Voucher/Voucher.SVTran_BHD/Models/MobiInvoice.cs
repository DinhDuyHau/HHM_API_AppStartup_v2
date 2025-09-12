using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voucher.SVTran_BHD.Models
{
    public class MobiInvoice
    {
        //Mã giao dịch (stt_rec)
        public string transCode { get; set; }

        //Loại đối tác SiS
        public string partnerType { get; set; }

        //Ngày phát sinh giao dịch (dd/MM/yyyy)
        public string transDate { get; set; }

        //Giá trị thuế (%) Mặc định 10
        public int vat { get; set; } = 10;

        //Tên Khách hàng
        public string custName { get; set; }

        //Địa chỉ KH
        public string address { get; set; }

        //Số điện thoại KH
        public string telNumber { get; set; } = "";

        //Mã số thuế KH
        public string tin { get; set; } = "";

        //Mã đơn vị bán
        public string shopCode { get; set; }

        //Tài khoản ngân hàng
        public string accountNo { get; set; } = "";

        //Tên ngân hàng
        public string bankName { get; set; } = "";

        //Số định danh cá  nhân
        public string identityNo { get; set; } = "";

        //Số hộ chiếu
        public string passportNo { get; set; } = "";

        //Mã đơn vị quan hệ ngân sách
        public string dvqhcsNo { get; set; } = "";

        //Danh sách hàng hóa
        public List<MobiInvDetail> lstItem { get; set; }

        public MobiInvoice()
        {
            lstItem = new List<MobiInvDetail>();
        }
    }
}
