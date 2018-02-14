using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;
using Client.Entities;

namespace Synchroniser.api
{
    [Route("api/[controller]")]
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly ITokenConsumer _crmClient;
        Contact _contact;

        public ContactController(ILogger<ContactController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
            _contact = new Contact((CRMClient)_crmClient);
        }

        [HttpGet("{email}")]
        public JsonResult Get(string email)
        {
            try
            {
                return Json(_contact.GetByEmail(email));
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                _logger.LogError("HTTP request failed: {0}", ex.ToString());
                _logger.LogError("Exception Type: ");
                _logger.LogError(ex.GetType().ToString());
                _logger.LogError("Exception: " + ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception is: {0}", ex.InnerException.GetType().ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Non-HTTP exception captured.");
                _logger.LogError(ex.ToString());
            }
            return Json("System error, see log");
        }
    }
}
