using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class OrderdetailController : ControllerBase<OrderController, Client.Entities.Orderdetail, Client.Types.OrderdetailSimple>
    {
        public OrderdetailController(ILogger<OrderController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Orderdetail((CRMClient)crmClient);
        }

        public async Task<IActionResult> List(Guid id)
        {
            var result = await entity.ListOrderedProducts(id);
            if (result != null)
            {
                return View(result);
            }
            return NotFound($"Order by id {id} has not been found.");
        }
    }
}
