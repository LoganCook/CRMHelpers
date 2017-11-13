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
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IADSearcher _aDSearcher;
        private readonly ITokenConsumer _crmClient;
        public string Message { get; set; }
        public List<Dictionary<string, string>> NewAccounts { get; private set; }

        [Required, BindProperty]
        public DateTime Earliest { get; set; } = new DateTime(DateTime.Now.Year, 01, 01);

        public IndexModel(ILogger<IndexModel> logger, IADSearcher aDSearcher, ITokenConsumer crmClient)
        {
            _logger = logger;
            _aDSearcher = aDSearcher;
            _crmClient = crmClient;
            NewAccounts = new List<Dictionary<string, string>>();
        }
        public IActionResult OnPost()
        {
            _logger.LogDebug($"OnPost has been called with {Earliest}");
           
            if (ModelState.IsValid)
            {
                _logger.LogDebug($"You set {Earliest}");
                NewAccounts = _aDSearcher.Search(Earliest);
                Message = $"You have set earliest date to {Earliest} and total number of new accounts created since then is {NewAccounts.Count}";
            }
            return Page();
        }
        /// <summary>
        /// Turn an email address to a unique html element id
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public string GetIdFromEamil(string emailAddress)
        {
            return "id" + emailAddress.Replace("@", "_").Replace(".", "_");
        }

        public string GetUserJSArray()
        {
            return Utils.GetJSArray(NewAccounts);
        }
    }
}
