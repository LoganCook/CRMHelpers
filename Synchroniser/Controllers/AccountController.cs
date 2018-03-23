using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Json;
using Client;

namespace Synchroniser.Controllers
{
    public class AccountController : ControllerBase<AccountController, Client.Entities.Account, Client.Types.Account>
    {
        public AccountController(ILogger<AccountController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Account((CRMClient)crmClient);
        }

        public async Task<IActionResult> ListTopAccounts()
        {
            // This view has a parent column, which is not helpful
            return View("index", await entity.ListTopAccounts());
        }

        public async Task<IActionResult> GetChildren(string parentName)
        {
            // This view has a parent column, which is not helpful
            return View("index", await entity.ListChildAccounts(parentName));
        }
    }
}
