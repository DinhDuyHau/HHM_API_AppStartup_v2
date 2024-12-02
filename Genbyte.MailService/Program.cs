using Genbyte.MailService.Model;
using Genbyte.MailService.Service;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

var mailService = new MailService(configuration);
var dataService = new DataService(configuration);

// danh sách mail nhận
var recipients = dataService.GetEmployeeCommissions();
//var recipients = new List<EmployeeCommission>
//{
//    new EmployeeCommission
//    {
//        ma_nvbh = "NV123",
//        ten_nvbh = "duydu5344",
//        email = "duydu5344@gmail.com",
//        tien_hh = 500000
//    },
//    new EmployeeCommission
//    {
//        ma_nvbh = "NV124",
//        ten_nvbh = "nduydu66",
//        email = "nduydu66@gmail.com",
//        tien_hh = 300000
//    },
//    new EmployeeCommission
//    {
//        ma_nvbh = "NV125",
//        ten_nvbh = "du.nd.2095",
//        email = "du.nd.2095@aptechlearning.edu.vn",
//        tien_hh = 700000
//    }
//};

// gọi hàm gửi email
await mailService.SendEmailAsync(recipients);

// Đợi người dùng nhấn phím trước khi đóng console
//Console.ReadKey();