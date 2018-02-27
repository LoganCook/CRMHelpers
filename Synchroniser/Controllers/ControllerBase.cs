//using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;
using System;

namespace Synchroniser.Controllers
{
    public class ControllerBase<UController> : Controller where UController : Controller
    {
        protected readonly ILogger<UController> _logger;
        protected readonly ITokenConsumer _crmClient;

        public ControllerBase(ILogger<UController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
        }
    }
}
