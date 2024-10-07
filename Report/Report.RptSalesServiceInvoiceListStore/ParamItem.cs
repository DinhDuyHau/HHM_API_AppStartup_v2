using Genbyte.Component.Report.Model;

namespace Report.RptSalesServiceInvoiceListStore
{
    public class ParamItem : ParamItemBase
    {
        public DateTime tu_ngay { get; set; }
        public DateTime den_ngay { get; set; }
        public string ma_dv { get; set; } = "";
        public string ma_cuahang{ get; set; } = "";
        public string ma_ca { get; set; } = "";
        public string ma_kh { get; set; } = "";
       

    }
}