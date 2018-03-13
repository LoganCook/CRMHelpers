using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class PricelistController : ControllerBase<OrderController, Client.Entities.Pricelist, Client.Types.Pricelist>
    {
        public PricelistController(ILogger<OrderController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Pricelist((CRMClient)crmClient);
        }
    }
}
