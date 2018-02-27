using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class OrderController : ControllerBase<OrderController>
    {
        private Client.Entities.Order order;

        public OrderController(ILogger<OrderController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            order = new Client.Entities.Order((CRMClient)crmClient);
        }

        public IActionResult Get()
        {
            return Ok("this is good");
        }
    }
}
