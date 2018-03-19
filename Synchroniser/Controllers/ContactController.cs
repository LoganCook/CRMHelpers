using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Json;
using Client;

namespace Synchroniser.Controllers
{
    public class ContactController : ControllerBase<ContactController, Client.Entities.Contact, Client.Types.Contact>
    {
        public ContactController(ILogger<ContactController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Contact((CRMClient)_crmClient);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            return await Get(id);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client.Types.ContactBase content)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Guid newID = await entity.Create<Client.Types.ContactBase>(content);
                    _logger.LogInformation($"Created new contact with id = {newID.ToString()}");
                    return RedirectToAction("Get", new { id = newID });
                } catch (HttpRequestException ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
            return View(content);
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
                    await entity.Update(content.ID, toUpdate);
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
                var result = await entity.GetByEmail(email);
                if (result != null)
                {
                    return Redirect(Url.Action("Get", new { id = new Guid(result.ID) }));
                }
            }
            return View();
        }

        /// <summary>
        /// Get orders of a Contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetOrders(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            Client.Entities.Order order = new Client.Entities.Order((CRMClient)_crmClient);
            var result = await order.ListOrdersOfContact(id);
            if (result != null)
            {
                return View(result);
            }
            return NotFound();
        }
    }
}
