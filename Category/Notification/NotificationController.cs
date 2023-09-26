using Genbyte.Base.CoreLib;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Notification.Model;
using Newtonsoft.Json.Linq;

namespace Notification
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        public NotificationController()
        {
        }

        /// <summary>
        /// Lấy thông báo
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_notification")]
        #region get_notification
        public IActionResult GetNotifications(string? status = "0", int page_index = 1, int page_size = 20)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                //lấy notification
                List<NotificationModel> list_notification = _service.GetNotifications(Startup.UserName, status, page_index, page_size);

                if (list_notification != null)
                {
                    model.success = true;
                    model.result = list_notification;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- NotificationController/GetNotifications/get_notification", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Lấy thông báo mới nhất theo người dùng
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_quantity_new_notification")]
        #region get_quantity_new_notification
        public IActionResult GetQuantityNewNotificaiton()
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                int count = _service.GetQuantityNewNotificaiton(Startup.UserName);
                model.success = true;
                model.result = count;
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- NotificationController/GetQuantityNewNotificaiton/", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Cập nhật số lượt thông báo mới nhất chưa hiển thị
        /// </summary>
        /// <returns></returns>
        [HttpPut("update_status_new_notification")]
        #region update_status_new_notification
        public IActionResult UpdateStatusNewNotification()
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                _service.UpdateStatusNewNotification(Startup.UserName);
                model.success = true;
                model.result = "sucess";
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- NotificationController/UpdateStatusNewNotification/", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Cập nhật số lượt thông báo mới nhất chưa hiển thị
        /// </summary>
        /// <returns></returns>
        [HttpPut("update_status_notification")]
        #region update_status_notification
        public IActionResult UpdateStatusNotification(decimal notification_id)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                _service.UpdateStatusNotification(Startup.UserName, notification_id);
                model.success = true;
                model.result = "sucess";
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- NotificationController/UpdateStatusNotification/", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Cập nhật token của người dùng
        /// </summary>
        /// <returns></returns>
        [HttpPut("update_token")]
        #region update_token
        public IActionResult UpdateToken(string token)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                _service.UpdateToken(Startup.UserName, token);
                model.success = true;
                model.result = "sucess";
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- NotificationController/UpdateToken/", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Xóa token của người dùng
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete_token")]
        #region delete_token
        public IActionResult DeleteToken()
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                _service.UpdateToken(Startup.UserName, "");
                model.success = true;
                model.result = "sucess";
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- NotificationController/DeleteToken/", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Gửi thông báo
        /// </summary>
        /// <returns></returns>
        [HttpPost("send_notification")]
        #region send_notification
        public IActionResult SendNotification(NotificationRequest request)
        {
            try
            {
                CommonObjectModel model = new CommonObjectModel()
                {
                    success = false,
                    message = "",
                    result = null
                };
                Service _service = new Service();
                _service.SendNotification(request);
                model.success = true;
                model.result = "sucess";
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"POST -- NotificationController/Sendnotification/", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
