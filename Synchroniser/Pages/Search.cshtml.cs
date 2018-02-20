using System;
using System.Collections.Generic;
using System.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ADConnectors;
using Client;

namespace Synchroniser.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ILogger<SearchModel> _logger;
        private readonly IADSearcher _aDSearcher;
        private readonly ITokenConsumer _crmClient;
        public string Message { get; set; }

        [Required, BindProperty, EmailAddress]
        public string Email { get; set; }

        public SearchModel(ILogger<SearchModel> logger, IADSearcher aDSearcher, ITokenConsumer crmClient)
        {
            _logger = logger;
            _aDSearcher = aDSearcher;
            _crmClient = crmClient;
        }

        public async Task<IActionResult> OnPost()
        {
            _logger.LogDebug($"OnPost has been called with {Email}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"You are looking for the contact by {Email}");
                var contact = new Client.Entities.Contact((CRMClient)_crmClient);
                var result = await contact.GetByEmail(Email);
                if (result != null)
                {
                    return Redirect($"/Entities/Contacts/Edit/{result.ID}");
                }
            }
            Message = $"No contact found by {Email}";
            return Page();
        }
    }
}
