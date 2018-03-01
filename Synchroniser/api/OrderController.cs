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
    public class OrderController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly ITokenConsumer _crmClient;
        Orderdetail order;

        public OrderController(ILogger<ContactController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
            order = new Orderdetail((CRMClient)_crmClient);
        }

        [HttpGet("OrderdetailRaw/{id}")]
        public async Task<ContentResult> Orderdetail(Guid id)
        {
            Console.WriteLine(id.ToString());
            string result = await order.ListOrderedProductsString(id);
            return Content(result, "application/json");
        }
    }
}
