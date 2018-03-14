using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class PropertyInstanceController : ControllerBase<PropertyInstanceController, Client.Entities.DynamicpropertyInstance, Client.Types.DynamicpropertyInstance>
    {
        public PropertyInstanceController(ILogger<PropertyInstanceController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.DynamicpropertyInstance((CRMClient)crmClient);
        }

        /// <summary>
        /// Get a list of properties of a product of an order line
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetProperties(Guid id)
        {
            return View(await entity.GetProductProperties(id));
        }
    }
}
