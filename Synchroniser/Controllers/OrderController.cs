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
    }
}
