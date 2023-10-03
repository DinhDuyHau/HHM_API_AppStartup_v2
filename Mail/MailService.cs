using Genbyte.Sys.Common;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mail.Model;

namespace Mail
{
    public class MailService
    {
        public void Send(MailRequest request, SmtpConfig smtpConfig)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(smtpConfig.EmailAddress));
            email.To.Add(MailboxAddress.Parse(request.send_to));
            email.Subject = request.title;

            // Create the email body (text part)
            var body = new TextPart(TextFormat.Html)
            {
                Text = request.body
            };

            // Create the attachment
            if (request.files != null)
            {
                // Create a multipart/mixed container to hold the email body and attachments
                var multipart = new Multipart("mixed");
                multipart.Add(body); // Thêm phần văn bản của email

                // Thêm các tệp đính kèm vào email
                foreach (var file in request.files)
                {
                    var attachment = new MimePart()
                    {
                        Content = new MimeContent(file.OpenReadStream(), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = file.FileName // Tên tệp đính kèm
                    };

                    multipart.Add(attachment); // Thêm tệp đính kèm vào multipart
                }

                // Set the email body to be the multipart container
                email.Body = multipart;
            }
            else
            {
                // No attachment, so set the email body directly
                email.Body = body;
            }

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(smtpConfig.Host, smtpConfig.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(smtpConfig.Username, smtpConfig.Password);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
