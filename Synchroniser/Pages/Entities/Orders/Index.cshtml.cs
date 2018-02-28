using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Client;
using Client.Types;

namespace Synchroniser.Pages.Entities.Orders
{
    // TODO: confirm to remove
    public class IndexModel : PageModel
    {
        private readonly ITokenConsumer _crmClient;
        public OrderBase order;
        public IndexModel(ITokenConsumer crmClient)
        {
            _crmClient = crmClient;
        }
        public async Task<IActionResult> OnGetAsync(string orderid)
        {
            Console.WriteLine($"Your orderid is {orderid}");
            var orderQuery = new Client.Entities.Order((CRMClient)_crmClient);
            var result = await orderQuery.List<Client.Types.OrderBase>(orderQuery.GetByOrderIDQuery(orderid));
            if (result != null && result.Count > 0)
            {
                order = result[0];
                return Page();
            }
            // FIXME: This NotFound is very rough: it says webpage cannot be found as a server response not an app response
            Console.WriteLine($"Not found {orderid}");
            return NotFound($"Order by id {orderid} has not been found.");
        }
    }
}