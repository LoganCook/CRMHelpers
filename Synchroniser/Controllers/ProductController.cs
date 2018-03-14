using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class ProductController : ControllerBase<ProductController, Client.Entities.Product, Client.Types.Product>
    {
        public ProductController(ILogger<ProductController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Product((CRMClient)crmClient);
        }

        /// <summary>
        /// Only return active products.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> List()
        {
            return View(await entity.ListAll<Client.Types.Product>("statecode eq 0"));
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            return View(await entity.GetDetail(id));
        }
    }
}
