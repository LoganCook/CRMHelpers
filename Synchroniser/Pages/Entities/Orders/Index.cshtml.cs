using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Synchroniser.Models;

namespace Synchroniser.Pages.Entities.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ITokenConsumer _crmClient;
        public Order order;
        public IndexModel(ITokenConsumer crmClient)
        {
            _crmClient = crmClient;
        }
        public async Task<IActionResult> OnGetAsync(string orderid)
        {
            Console.WriteLine($"Your orderid is {orderid}");
            Stream response = await _crmClient.GetStreamAsync(Order.GetByOrderID(orderid));
            if (response == null)
            {
                return NotFound($"Dynamic server returned invalid response: empty");
            }
            OData<Order> odata = CRMClient.DeserializeObject<OData<Order>>(response);
            if (odata.Value is List<Order> && odata.Value.Count > 0)
            {
                Console.WriteLine($"{odata.Value.Count} results have been found, only one returned to user");
                order = odata.Value[0];
                Console.WriteLine(order.Name);
                return Page();
            }

            // FIXME:
            // This NotFound is very rough: it says webpage cannot be found as a server response not an app response
            Console.WriteLine($"Not found {orderid}");
            return NotFound($"Order by id {orderid} has not been found.");
        }
    }
}