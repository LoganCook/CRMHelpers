using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;

namespace Synchroniser.Controllers
{
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly ITokenConsumer _crmClient;

        public ContactController(ILogger<ContactController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
        }

        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var contact = new Client.Entities.Contact((CRMClient)_crmClient);
            var result = await contact.Get<Client.Types.Contact>(id);
            if (result != null)
            {
                return View(result);
            }
            return NotFound();
        }
    }
}
