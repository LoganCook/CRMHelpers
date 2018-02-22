using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Json;
using Client;
using Client.Entities;

namespace Synchroniser.Pages.Entities.Contacts
{
    public class ContactModel : PageModel
    {
        private readonly ITokenConsumer _crmClient;

        //[BindProperty]
        public Client.Types.Contact Contact { get; set; }

        public ContactModel(ITokenConsumer crmClient)
        {
            _crmClient = crmClient;
        }

        public async Task<IActionResult> OnGet(Guid id)
        {
            Console.WriteLine($"You requested {id}");
            Stream response = await _crmClient.GetStreamAsync($"contacts({id})");
            if (response == null)
            {
                return NotFound("Contact does not exist.");
            }
            Contact = CRMClient.DeserializeObject<Client.Types.Contact>(response);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            foreach (string k in  Request.Form.Keys)
            {
                Console.WriteLine($"{k}: {Request.Form[k]}");
            }
            //string id = "d8bd71c5-5b63-e611-80e3-c4346bc43f98";
            //if (ModelState.IsValid)
            //{
            //    Console.WriteLine($"To update: {Contact.Id}");
            //    Console.WriteLine($"your new department is: {Contact.Department}");
            //}
            //Console.WriteLine($"To update: {Contact.Id}");
            //Console.WriteLine($"your new department is: {Contact.Department}");
            //return RedirectToPage("/Entities/Contacts/Edit/" + id);
            Console.WriteLine($"To update: {Request.Form["Contact.Id"]}");
            Console.WriteLine($"your new department is: {Request.Form["Contact.Department"]}");

            // Server side validation: how to tell what data are wrong?
            if (!string.IsNullOrEmpty(Request.Form["Contact.Department"].ToString()))
            {
                // just update what we need, do not send everything back
                // https://msdn.microsoft.com/en-us/library/mt607664.aspx
                // Cannot use DataContract because selected properties to be updated. Really?
                JsonObject toUpdate = (JsonObject)JsonValue.Parse("{}");
                toUpdate["department"] = Request.Form["Contact.Department"].ToString();
                Contact contact = new Contact((CRMClient)_crmClient);
                await contact.Update(id.ToString(), toUpdate);
            }
            return await OnGet(id);
        }
    }
}