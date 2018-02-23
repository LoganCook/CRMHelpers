using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Json;
using Client;

namespace Synchroniser.Controllers
{
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly ITokenConsumer _crmClient;
        private Client.Entities.Contact contact;

        public ContactController(ILogger<ContactController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
            contact = new Client.Entities.Contact((CRMClient)_crmClient);
        }

        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            
            var result = await contact.Get<Client.Types.Contact>(id);
            if (result != null)
            {
                return View(result);
            }
            return NotFound();
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            return await Get(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client.Types.Contact content)
        {
            if (ModelState.IsValid)
            {
                _logger.LogDebug($"To update Contact: {content.ID}");
                Console.WriteLine($"your new department is: {content.Department}");

                // Server side validation: how to tell what data are wrong?
                if (!string.IsNullOrEmpty(content.Department))
                {
                    // just update what we need, do not send everything back
                    // https://msdn.microsoft.com/en-us/library/mt607664.aspx
                    // Cannot use DataContract because selected properties to be updated. Really?
                    JsonObject toUpdate = (JsonObject)JsonValue.Parse("{}");
                    toUpdate["department"] = content.Department;
                    await contact.Update(content.ID, toUpdate);
                    _logger.LogDebug($"Updated the Department to {content.Department} for {content.ID}");
                }
            }
            return await Edit(new Guid(content.ID));
        }

        public async Task<IActionResult> Search(string email)
        {
            _logger.LogDebug($"ContactController::Search has been called with {email}");
            if (!string.IsNullOrEmpty(email))
            {
                _logger.LogDebug($"You are looking for the contact by {email}");
                var result = await contact.GetByEmail(email);
                if (result != null)
                {
                    return Redirect(Url.Action("Get", new { id = new Guid(result.ID) }));
                }
            }
            return View();
        }
    }
}
