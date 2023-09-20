using Employee.Model;
using Genbyte.Base.CoreLib;
using Genbyte.Base.Security;
using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly Security _security;
        public EmployeeController(IOptions<Security> security)
        {
            this._security = security.Value;
        }

        /// <summary>
        /// Lấy danh sách ASM
        /// </summary>
        [HttpGet("get_list_asm")]
        #region get_payment_debit
        public IActionResult GetListASM(string? ma_nvbh = "", string? order_by = "", int page_index = 1, int page_size = 0)
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

                //check injection
                if (!_service.IsSQLInjectionValid(ma_nvbh) || !_service.IsSQLInjectionValid(order_by))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                EntityCollection<EmployeeModel> employees = _service.GetListEmployee(ma_nvbh ?? "", order_by ?? "", page_index, page_size);
                if (employees != null)
                {
                    model.success = true;
                    model.result = employees;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/GetListASM?ma_nvbh={ma_nvbh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Lấy ASM bằng mã
        /// </summary>
        [HttpGet("get_asm_by_id")]
        #region get_payment_debit
        public IActionResult GetASMById(string? ma_nvbh = "")
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

                //check injection
                if (!_service.IsSQLInjectionValid(ma_nvbh))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                EntityCollection<EmployeeModel> employees = _service.GetListEmployee(ma_nvbh ?? "", "", 1, 1);
                if (employees != null)
                {
                    model.success = true;
                    model.result = (employees.Items != null && employees.Items.Count > 0) ? employees.Items[0] : null;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/GetListASM?ma_nvbh={ma_nvbh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
