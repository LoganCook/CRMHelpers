using System;
using System.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Client;
using Client.Types;

namespace Synchroniser.Pages.Entities.Contacts
{
    public class CreateModel : PageModel
    {
        private readonly ITokenConsumer _crmClient;

        [BindProperty]
        public ContactBase Contact { get; set; }

        public CreateModel(ITokenConsumer crmClient)
        {
            _crmClient = crmClient;
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Console.WriteLine("Create method has been called without AccountID set");
            HttpResponseMessage result = await _crmClient.SendJsonAsync<JsonObject>(HttpMethod.Post, ContactBase.URI, Contact.Create());
            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                string newOrderID = result.Headers.GetValues("OData-EntityId").FirstOrDefault();
                Console.WriteLine("New entity: {0}", newOrderID);
            } else
            {
                Utils.DisplayResponse(result);
            }

            return RedirectToPage("/Index");
        }
    }
}