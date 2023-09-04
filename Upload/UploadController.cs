using Genbyte.Sys.AppAuth;
using Genbyte.Sys.Common;
using Genbyte.Sys.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upload.Models;

namespace Upload
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UploadController: ControllerBase
    {
        public UploadController() { }

        [HttpPost("save_customer_image")]
        public CommonObjectModel saveImageCustomer([FromForm] CustomerUpload request)
        {
            CommonObjectModel respose = new CommonObjectModel();
            respose.message = "";
            respose.success = false;

            Service service = new Service();
            try
            {
                respose.result = service.saveImageCustomer(request);
                respose.success = true;
                respose.message = "success";
            }
            catch (CustomException ex)
            {
                respose.success = false;
                respose.message = ex.Message;
            }
            catch (Exception ex)
            {
                respose.success = false;
                respose.message = ApiReponseMessage.Error_Runtime;
            }
            return respose;
        }
        [HttpPost("save_wholesale_contract")]
        public CommonObjectModel saveWholesale([FromForm] WholeSaleUpload request)
        {
            CommonObjectModel respose = new CommonObjectModel();
            respose.message = "";
            respose.success = false;
            Service service = new Service();
            try
            {
                respose.result = service.saveWholesaleContract(request);
                respose.success = true;
                respose.message = "success";
            }
            catch (CustomException ex)
            {
                respose.success = false;
                respose.message = ex.Message;
            }
            catch (Exception ex)
            {
                respose.success = false;
                respose.message = ApiReponseMessage.Error_Runtime;
            }
            return respose;
        }
        [HttpDelete("delete_wholesale_contract")]
        public CommonObjectModel deleteWholesale([Required] string so_ct, [Required] DateTime ngay_ct)
        {
            CommonObjectModel respose = new CommonObjectModel();
            respose.message = "";
            respose.success = false;
            Service service = new Service();
            try
            {
                service.deleteWholesaleContract(so_ct, ngay_ct);
                respose.success = true;
                respose.message = "success";
            }
            catch (CustomException ex)
            {
                respose.success = false;
                respose.message = ex.Message;
            }
            catch (Exception ex)
            {
                respose.success = false;
                respose.message = ApiReponseMessage.Error_Runtime;
            }
            return respose;
        }
    }
}
