using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common.Models;
using Genbyte.Sys.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Genbyte.Base.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Mail.Model;

namespace Mail
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {

        private readonly SmtpConfig _smtpConfig;
        public  MailController(IOptions<SmtpConfig> smtpConfig)
        {
            _smtpConfig = smtpConfig.Value;
        }

        /// <summary>
        /// Gửi email
        /// </summary>
        [HttpPost("send_email")]
        #region send_email
        public IActionResult SendEmail([FromForm] MailRequest request)
        {
            CommonObjectModel model = new CommonObjectModel
            {
                message = "send_email_error",
                result = null,
                success = false,
            };

            MailService mailService = new MailService();
            try
            {
                mailService.Send(request, _smtpConfig);
            }
            catch (Exception ex)
            {
                return Ok(model);
            }
            model.success = true;
            model.message = "send_email_success";
            return Ok(model);
        }
        #endregion
    }
}
