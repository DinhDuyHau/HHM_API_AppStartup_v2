using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using MenuRight.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MenuRight
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MenuRightController : ControllerBase
    {
        private readonly Security _security;

        public MenuRightController(IOptions<Security> security)
        {
            this._security = security.Value;
        }

        /// <summary>
        /// Lấy menu phân quyền cho tài khoản đăng nhập 
        /// </summary>
        /// <returns>MenuModal</returns>
        [HttpGet("get_menu_rights")]
        #region get_menu_rights
        public IActionResult GetMenuRight()
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

                //lấy menu right
                List<MenuModal> menus = _service.GetMenuRight().ToList();
                if (menus != null)
                {
                    model.success = true;
                    model.result = menus;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- MenuRightController/GetMenuRight", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
