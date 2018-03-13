﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client;

namespace Synchroniser.Controllers
{
    /// <summary>
    /// Base class for controllers. It has actions: Get, Index
    /// </summary>
    /// <typeparam name="UController"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="WType"></typeparam>
    public class ControllerBase<UController, TEntity, WType> : Controller where UController : Controller
                                                                   where TEntity : Client.Entities.Base 
    {
        protected readonly ILogger<UController> _logger;
        protected readonly ITokenConsumer _crmClient;
        protected TEntity entity;

        public ControllerBase(ILogger<UController> logger, ITokenConsumer crmClient)
        {
            _logger = logger;
            _crmClient = crmClient;
        }

        public async Task<IActionResult> Index()
        {
            return View(await entity.ListAll<WType>());
        }

        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var result = await entity.Get<WType>(id);
            if (result != null)
            {
                return View(result);
            }
            return NotFound();
        }
    }
}
