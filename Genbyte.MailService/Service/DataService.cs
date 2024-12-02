using System.Data.SqlClient;
using Genbyte.MailService.Model;
using Microsoft.Extensions.Configuration;

namespace Genbyte.MailService.Service
{
    public class DataService
    {
        private readonly string _connectionString;
        private readonly LoggingService _loggingService;

        public DataService(IConfiguration configuration)
        {
            var logDirectory = configuration.GetSection("SendGrid:LogDirectory").Value;
            _connectionString = configuration.GetConnectionString("App");
            _loggingService = new LoggingService(logDirectory);
        }

        /// <summary>
        /// Kiểm tra kết nối
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _loggingService.WriteLogAsync($"Lỗi khi kết nối đến SQL Server: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra select dữ liệu từ bảng dmkh, lấy top 10 bản ghi
        /// </summary>
        public void TestQuery()
        {
            string query = "SELECT TOP 10 * FROM dmkh";  // Câu truy vấn SQL

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open(); // Mở kết nối

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) // Thực thi câu truy vấn và lấy dữ liệu
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            Console.WriteLine($"ma_kh: {reader["ma_kh"]}, ten_kh: {reader["ten_kh"]}");
                            count++;
                        }

                        if (count == 0)
                        {
                            Console.WriteLine("No records found in the table.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL query: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách hoa hồng của nhân viên bán hàng từ bảng QStore_hoahong_nvbh
        /// </summary>
        public List<EmployeeCommission> GetEmployeeCommissions()
        {
            string query = "SELECT ma_nvbh, ten_nvbh, email, tien_hh, cr_date FROM QStore_hoahong_nvbh";
            var employeeCommissions = new List<EmployeeCommission>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open(); // Mở kết nối

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) // Thực thi câu truy vấn và lấy dữ liệu
                    {
                        while (reader.Read())
                        {
                            var commission = new EmployeeCommission
                            {
                                ma_nvbh = reader["ma_nvbh"].ToString(),
                                ten_nvbh = reader["ten_nvbh"].ToString(),
                                email = reader["email"].ToString(),
                                tien_hh = reader.IsDBNull(reader.GetOrdinal("tien_hh")) ? 0 : reader.GetDecimal(reader.GetOrdinal("tien_hh")),
                                cr_date = reader.IsDBNull(reader.GetOrdinal("cr_date")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("cr_date"))
                            };

                            employeeCommissions.Add(commission);
                        }

                        if (employeeCommissions.Count == 0)
                        {
                            _loggingService.WriteLogAsync("Không tìm thấy bản ghi nào trong QStore_hoahong_nvbh");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.WriteLogAsync($"Lỗi khi thực hiện truy vấn SQL: {ex.Message}");
            }

            return employeeCommissions;
        }
    }
}
