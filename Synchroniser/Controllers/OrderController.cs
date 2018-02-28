using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class OrderController : ControllerBase<OrderController, Client.Entities.Order, Client.Types.Order>
    {
        public OrderController(ILogger<OrderController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Order((CRMClient)crmClient);
        }

        // Get an order by its Order ID
        public async Task<IActionResult> GetByID(string ID)
        {
            var result = await entity.List<Client.Types.OrderBase>(entity.GetByOrderIDQuery(ID));
            if (result != null && result.Count > 0)
            {
                return View("Get", result[0]);
            }
            return NotFound($"Order by id {ID} has not been found.");
        }
    }
}
