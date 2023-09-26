using Genbyte.Base.CoreLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Model;
using Genbyte.Sys.AppAuth;
using System.Collections;
using Genbyte.Notification;

namespace Notification
{
    public class Service : CoreService
    {
        public int GetQuantityNewNotificaiton(string user_name)
        {
            string sql = @"SELECT COUNT(1) as count FROM user_notification WHERE user_name = @user_name AND status = '0' ";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_name",
                SqlDbType = SqlDbType.Char,
                Value = user_name
            });
            return (int) base.ExecSql2DataSet(sql, paras, ConnectType.App).Tables[0].Rows[0]["count"];
        }
        public void UpdateStatusNewNotification(string user_name)
        {
            string sql = @"update user_notification set status = '1' WHERE user_name = @user_name";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_name",
                SqlDbType = SqlDbType.Char,
                Value = user_name
            });
            base.ExecuteNonQuery(sql, paras, ConnectType.App);
        }
        public void UpdateStatusNotification(string user_name, decimal notification_id)
        {
            string sql = @"update user_notification set status = '2' WHERE user_name = @user_name and notification_id = @notification_id";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_name",
                SqlDbType = SqlDbType.Char,
                Value = user_name
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@notification_id",
                SqlDbType = SqlDbType.Decimal,
                Value = notification_id
            });
            base.ExecuteNonQuery(sql, paras, ConnectType.App);
        }
        public List<NotificationModel> GetNotifications(string user_name, string status, int page_index, int page_size)
        {
            string sql = @"select a.status, b.* from user_notification a left join notification b on a.notification_id = b.notification_id 
                           where a.user_name = @user_name ";
            if (status == "1") sql += " and a.status = @status ";
            sql += $"order by send_date desc offset {(page_index - 1) * page_size} rows fetch next {page_size} rows only";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_name",
                SqlDbType = SqlDbType.Char,
                Value = user_name
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@status",
                SqlDbType = SqlDbType.Char,
                Value = status
            });
            return base.ExecSql2List<NotificationModel>(sql, paras, ConnectType.App);
        }
        public void UpdateToken(string user_name, string device_token)
        {
            string sql = @"update userinfo2 set device_token = @device_token WHERE name = @user_name";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_name",
                SqlDbType = SqlDbType.Char,
                Value = user_name
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@device_token",
                SqlDbType = SqlDbType.NVarChar,
                Value = device_token
            });
            base.ExecuteNonQuery(sql, paras, ConnectType.Sys);
        }

        public void SendNotification(NotificationRequest request)
        {
            List<UserToken> tokens = this.GetDeviceToken(request.send_to);
            // Lưu thông báo vào cơ sở dữ liệu
            string sql = "declare @notification_id decimal \n";
            sql += $"insert into notification(send_from, title, body, send_date, status, datetime0, redirect) values ('{request.send_from}', N'{request.title}', N'{request.body}', GETDATE(), '1', GETDATE(), N'{request.data["redirect"]}') \n";
            sql += $"select @notification_id = SCOPE_IDENTITY() \n";
            foreach (var user_name in request.send_to)
            {
                sql += $"insert into user_notification(user_name, notification_id, status) values ('{user_name}', @notification_id, '0') \n";
            }
            sql += "select @notification_id as notification_id";
            decimal notification_id = (decimal) base.ExecTransactionSql2DataSet(sql).Tables[0].Rows[0]["notification_id"];
            request.data.Add("notification_id", notification_id.ToString());
            NotificationService notificationService = new NotificationService();
            List<string> deviceError = notificationService.SendMessageAsync(request.title, request.body, tokens.Select(x=>x.device_token).ToList(), request.data, request.imageUrl).Result;
        }

        private string getGroupListByUser(string user_name)
        {
            string sql = @"SELECT dbo.ff_GetGroupList(@user_name) as group_list";
            List<SqlParameter> paras = new List<SqlParameter>();
            paras.Add(new SqlParameter()
            {
                ParameterName = $"@user_name",
                SqlDbType = SqlDbType.Char,
                Value = user_name
            });
            return base.ExecSql2DataSet(sql, paras, ConnectType.Sys).Tables[0].Rows[0]["group_list"].ToString();
        }
        public List<UserToken> GetDeviceToken(List<string> customers)
        {
            List<UserToken> userToken = new List<UserToken>();
            string str_customers = string.Join(",", customers);
            try
            {
                string sql = "SELECT name, device_token from userinfo2 where dbo.ff_ExactInlist(name, @str_customers) = 1 and isnull(device_token, '') <> ''";

                List<SqlParameter> paras = new List<SqlParameter>();
                paras.Add(new SqlParameter()
                {
                    ParameterName = "@str_customers",
                    SqlDbType = SqlDbType.Char,
                    Value = str_customers
                });
                userToken = base.ExecSql2List<UserToken>(sql, paras, ConnectType.Sys);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return userToken;
        }
    }
}
