namespace Imei
{
    public class VoucherDetail
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Detail_Type { get; set; }

        public List<DetailEntity> Data { get; set; }
    }
}
