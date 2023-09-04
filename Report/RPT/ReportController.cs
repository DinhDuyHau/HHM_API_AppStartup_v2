using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Report;
using Report.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ReportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Lấy file pdf
        /// </summary>
        /// <param name="key">stt_rec của chứng từ</param>
        /// <param name="controller">Controller xử lý</param>
        /// <param name="form_id">Mã mẫu in</param>
        /// <returns></returns>
        ///
        #region get_pdf_voucher
        [HttpGet("get_pdf_voucher")]
        public IActionResult GetPdfVoucher(string key, string controller, string form_id)
        {
            string sysid = _configuration["Report:Sysid"];
            string service_url = _configuration["Report:Service"];

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
                if (!_service.IsSQLInjectionValid(key) || !_service.IsSQLInjectionValid(controller) || !_service.IsSQLInjectionValid(form_id))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy pdf
                string pdf_voucher = "";
                try
                {
                    pdf_voucher = _service.getPdf(sysid, service_url, Startup.Unit, key, controller, form_id);
                }
                catch (Exception ex)
                {
                    model.success = false;
                    model.result = null;
                    model.message = "Runtime_err";
                    return Ok(model);
                }
                if (pdf_voucher != null)
                {
                    model.success = true;
                    model.result = "data:application/pdf;base64," + pdf_voucher;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ReportController/GetPdfVoucher?key={key}&controller={controller}&form_id={form_id}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion

        /// <summary>
        /// Lấy danh sách các mẫu in
        /// </summary>
        /// <param name="key">stt_rec của chứng từ</param>
        /// <param name="controller">Controller xử lý</param>
        /// <param name="form_id">Mã mẫu in</param>
        /// <returns></returns>
        ///
        #region get_menu_report
        [HttpGet("get_menu_report/{sysid}")]
        public IActionResult GetMenuReport(string sysid)
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
                if (!_service.IsSQLInjectionValid(sysid))
                    return BadRequest(new { message = ApiReponseMessage.Error_InputData });

                //lấy pdf
                List<ReportMenu> reportMenus = new List<ReportMenu>();
                try
                {
                    reportMenus = _service.getReportMenu(sysid);
                }
                catch (Exception ex)
                {
                    model.success = false;
                    model.result = null;
                    model.message = "Runtime_err";
                    return Ok(model);
                }
                if (reportMenus != null)
                {
                    model.success = true;
                    model.result = reportMenus;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ReportController/GetMenuReport?sysid={sysid}", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        [HttpGet("get_menu_report")]
        public IActionResult GetAllMenuReport()
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
                Dictionary<string, List<ReportMenu>> menu = new Dictionary<string, List<ReportMenu>>();
                try
                {
                    List<ReportMenu> reportMenus = _service.getReportMenu();
                    reportMenus.ForEach((item) =>
                    {
                        item.controller = item.controller.Trim();
                        item.sysid = item.sysid.Trim();
                        item.form_id = item.form_id.Trim();
                        if (!menu.ContainsKey(item.sysid))
                        {
                            menu[item.sysid] = new List<ReportMenu>{item};
                        }
                        else
                        {
                            menu[item.sysid].Add(item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    model.success = false;
                    model.result = null;
                    model.message = "Runtime_err";
                    return Ok(model);
                }
                if (menu != null)
                {
                    model.success = true;
                    model.result = menu;
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Insert(Startup.Unit, $"GET -- ReportController/GetMenuReport", ex);
                return BadRequest(new { message = ApiReponseMessage.Error_Runtime });
            }
        }
        #endregion
    }
}
