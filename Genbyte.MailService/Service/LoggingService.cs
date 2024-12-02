namespace Genbyte.MailService.Service
{
    public class LoggingService
    {
        private readonly string _logDirectory;

        public LoggingService(string logDirectory)
        {
            // Đảm bảo thư mục lưu log tồn tại
            _logDirectory = logDirectory;
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Ghi log vào file theo ngày.
        /// </summary>
        /// <param name="message">Nội dung log cần ghi</param>
        public async Task WriteLogAsync(string message)
        {
            // Tạo tên file log theo ngày hiện tại (vd: log-2024-12-02.txt)
            var logFilePath = Path.Combine(_logDirectory, $"log-{DateTime.Now:yyyy-MM-dd}.txt");

            // Ghi log vào file
            try
            {
                using (var writer = new StreamWriter(logFilePath, append: true))
                {
                    var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
                    await writer.WriteLineAsync(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi ghi log: {ex.Message}");
            }
        }
    }
}
