using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;
using Genbyte.MailService.Model;

namespace Genbyte.MailService.Service
{
    public class MailService
    {
        private readonly string _apiKey;
        private readonly string _mailfrom;
        private readonly string _sender;
        private readonly string _templateFilePath;
        private readonly string _subject;
        private readonly LoggingService _loggingService;

        public MailService(IConfiguration configuration)
        {
            var logDirectory = configuration.GetSection("SendGrid:LogDirectory").Value;
            _apiKey = configuration.GetSection("SendGrid:ApiKey").Value;
            _mailfrom = configuration.GetSection("SendGrid:MailFrom").Value;
            _sender = configuration.GetSection("SendGrid:Sender").Value;
            _templateFilePath = configuration.GetSection("SendGrid:TemplateFile").Value;
            _subject = configuration.GetSection("SendGrid:Subject").Value;
            _loggingService = new LoggingService(logDirectory);
        }

        /// <summary>
        /// Thực hiện gửi email theo danh sách truyền vào.
        /// - Gửi từng email cá nhân hóa đến từng nhân viên.
        /// - Ghi log kết quả gửi email (thành công/thất bại) vào file txt.
        /// - File log được tạo theo ngày, mỗi ngày sẽ ghi vào một file riêng biệt.
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(List<EmployeeCommission> recipients)
        {
            try
            {
                // Đọc template từ file JSON
                var template = await File.ReadAllTextAsync(_templateFilePath);

                // Tạo client SendGrid với API key
                var client = new SendGridClient(_apiKey);

                // Tạo thông tin người gửi (From) và người nhận (To)
                var from = new EmailAddress(_mailfrom, _sender);

                foreach (var recipient in recipients)
                {
                    try
                    {
                        // Áp dụng các giá trị động vào template HTML cho từng nhân viên
                        var emailContent = ApplyTemplate(template, recipient);

                        // Tạo email với nội dung HTML
                        var to = new EmailAddress(recipient.email);
                        var msg = MailHelper.CreateSingleEmail(from, to, _subject, null, emailContent);

                        // Gửi email
                        var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

                        // Kiểm tra mã phản hồi từ SendGrid (Accepted là thành công)
                        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            var successLog = $"Email đã được gửi thành công đến {recipient.email} ({recipient.ten_nvbh})";
                            await _loggingService.WriteLogAsync(successLog);
                        }
                        else
                        {
                            var failureLog = $"Gửi email thất bại đến {recipient.email} ({recipient.ten_nvbh}). Status code: {response.StatusCode}";
                            await _loggingService.WriteLogAsync(failureLog);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        // Ghi log khi có lỗi trong quá trình gửi email
                        var errorLog = $"Lỗi khi gửi email đến {recipient.email} ({recipient.ten_nvbh}): {innerEx.Message}";
                        await _loggingService.WriteLogAsync(errorLog);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi tổng thể vào file log
                var overallErrorLog = $"Lỗi sendgrid: {ex.Message}";
                await _loggingService.WriteLogAsync(overallErrorLog);
            }
        }

        /// <summary>
        /// Tách list email ra từng group nhỏ, phòng trường hợp list quá lớn
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="groupSize"></param>
        /// <returns></returns>
        public List<List<EmployeeCommission>> SplitEmails(List<EmployeeCommission> recipients, int groupSize)
        {
            var emailGroups = new List<List<EmployeeCommission>>();
            for (int i = 0; i < recipients.Count; i += groupSize)
            {
                emailGroups.Add(recipients.GetRange(i, Math.Min(groupSize, recipients.Count - i)));
            }
            return emailGroups;
        }

        /// <summary>
        /// Áp dụng template html
        /// </summary>
        /// <param name="template"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        private string ApplyTemplate(string template, EmployeeCommission employee)
        {
            var replacements = new Dictionary<string, string>
            {
                { "ma_nvbh", employee.ma_nvbh },
                { "ten_nvbh", employee.ten_nvbh },
                { "tien_hh", employee.tien_hh.ToString("N0") + " VND" }, // Định dạng tiền hoa hồng
                { "cr_date", employee.cr_date.ToString("dd/MM/yyyy") }
            };

            foreach (var replacement in replacements)
            {
                template = template.Replace("{" + replacement.Key + "}", replacement.Value);
            }
            return template;
        }

    }
}
