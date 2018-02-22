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
    public class AccountController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly ITokenConsumer _crmClient;
        Account _account;

        public AccountController(ILogger<ContactController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
            _account = new Account((CRMClient)_crmClient);
        }

        [HttpGet("ChildAccountsRaw/{name}")]
        public async Task<ContentResult> ChildAccountsRaw(string name)
        {
            string result = await _account.ListChildAccountsString(name);
            return Content(result, "application/json");
        }

        [HttpGet("ChildAccounts/{name}")]
        public async Task<List<Client.Types.Account>> ChildAccounts(string name)
        {
            List<Client.Types.Account> result = await _account.ListChildAccounts(name);
            return result;
        }
    }
}
