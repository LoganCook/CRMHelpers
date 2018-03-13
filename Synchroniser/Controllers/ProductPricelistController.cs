using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class ProductPricelistController : ControllerBase<ProductPricelistController, Client.Entities.ProductPricelist, Client.Types.ProductPricelist>
    {
        public ProductPricelistController(ILogger<ProductPricelistController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.ProductPricelist((CRMClient)crmClient);
        }
    }
}
