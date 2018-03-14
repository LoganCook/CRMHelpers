using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;


namespace Synchroniser.Controllers
{
    public class DynamicpropertyController : ControllerBase<DynamicpropertyController, Client.Entities.Dynamicproperty, Client.Types.Dynamicproperty>
    {
        public DynamicpropertyController(ILogger<DynamicpropertyController> logger, ITokenConsumer crmClient) : base(logger, crmClient)
        {
            entity = new Client.Entities.Dynamicproperty((CRMClient)crmClient);
        }

        /// <summary>
        /// Only return active properties.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> List()
        {
            return View(await entity.ListAll<Client.Types.Dynamicproperty>("statecode eq 0"));
        }
    }
}
