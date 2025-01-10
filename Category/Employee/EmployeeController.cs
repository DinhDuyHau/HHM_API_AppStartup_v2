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
        /// Lấy danh sách người duyệt
        /// </summary>
        [HttpGet("get_list_approver")]
        #region get_list_approver
        public IActionResult GetListApprover(string? ma_nvbh = "", string? order_by = "", int page_index = 1, int page_size = 0)
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
                EntityCollection<EmployeeModel> employees = _service.GetListEmployeeApprover(ma_nvbh ?? "", order_by ?? "", page_index, page_size);
                if (employees != null)
                {
                    model.success = true;
                    model.result = employees;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/GetListApprover?ma_nvbh={ma_nvbh}", ex);
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
        /// <summary>
        /// Lấy danh sách ban giám đốc để duyệt chiết khấu ngoại giao
        /// </summary>
        [HttpGet("get_list_approver_discount")]
        #region get_list_apporove_discount
        public IActionResult GetListBGD(string? name = "", string? order_by = "", int page_index = 1, int page_size = 0)
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
                if (!_service.IsSQLInjectionValid(name) || !_service.IsSQLInjectionValid(order_by))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                EntityCollection<UserModel> employees = _service.GetListUser(name ?? "", "BGD", order_by ?? "", page_index, page_size);
                if (employees != null)
                {
                    model.success = true;
                    model.result = employees;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/GetListBGD?name={name}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
        /// <summary>
        /// Lấy ASM bằng mã
        /// </summary>
        [HttpGet("get_approver_discount_by_id")]
        #region get_payment_debit
        public IActionResult GetBGDById(string? name = "")
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
                if (!_service.IsSQLInjectionValid(name))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                EntityCollection<UserModel> employees = _service.GetListUser(name ?? "", "BGD", "", 1, 1);
                if (employees != null)
                {
                    model.success = true;
                    model.result = (employees.Items != null && employees.Items.Count > 0) ? employees.Items[0] : null;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/GetBGDById?name={name}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpGet("get_employee_by_name")]
        #region get_employee_by_name
        public IActionResult GetEmployee(string username, string ma_cuahang)
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
                if (!_service.IsSQLInjectionValid(username) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy công nợ
                EmployeeModel employee = _service.GetEmployee(username, ma_cuahang);
                if (employee != null)
                {
                    model.success = true;
                    model.result = employee;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/get_employee_by_name?username={username}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        [HttpGet("get_department_by_name")]
        #region get_employee_by_name
        public IActionResult GetDepartment(string ma_kh, string ma_cuahang)
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
                if (!_service.IsSQLInjectionValid(ma_kh) || !_service.IsSQLInjectionValid(ma_cuahang))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy bộ phận
                DepartmentModel employee = _service.GetDepartment(ma_kh, ma_cuahang);
                if (employee != null)
                {
                    model.success = true;
                    model.result = employee;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- EmployeeController/get_department_by_name?username={ma_kh}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

    }
}
