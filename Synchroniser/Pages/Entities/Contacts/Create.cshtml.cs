using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Synchroniser.Models;

namespace Synchroniser.Pages.Entities.Contacts
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Contact Contact { get; set; }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Need to remove contactid for creating
            Console.WriteLine(CRMClient.SerializeObject<Contact>(Contact));
            return RedirectToPage("/Index");
        }
    }
}