using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
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

        public async Task<IActionResult> GetByID(Guid id)
        {
            var result = await entity.List<Client.Types.OrderdetailSimple>(entity.GetByIDQuery(id.ToString()));
            if (result != null && result.Count > 0)
            {
                return View("../Orderdetail/List", result);
            }
            return NotFound($"Order by id {id} has not been found.");
        }

        // Get an order by its Order ID
        public async Task<IActionResult> GetByOrderID(string id)
        {
            var result = await entity.List<Client.Types.OrderWithProducts>(entity.GetByOrderIDQuery(id));
            if (result != null && result.Count > 0)
            {
                result[0].OrderDetails = await entity.NextLink<Client.Types.OrderdetailSimple>(result[0].OrderDetailsLink);
                return View("GetMore", result[0]);
            }
            return NotFound($"Order by id {id} has not been found.");
        }

        public async Task<IActionResult> GetByAccountID(Guid id)
        {
            return View("OfAccount", await entity.ListOrdersOfAccount(id));
        }

        public IActionResult Create()
        {
            Client.Types.Order4Creation dummy = new Client.Types.Order4Creation
            {
                CustomerID = "84cc87f3-ad62-e611-80e3-c4346bc516e8",
                PriceLevelID = "0c407dd9-1b59-e611-80e2-c4346bc58784",
                Name = "A test order created by ASP.NET",
                Description = "Created by code, delete it after use",
                OrderID = "test 1111",
                OrderedProducts = new List<Client.Types.OrderDetail> {
                    new Client.Types.OrderDetail { Quantity = 1, ProductID = "09e6d87e-46ef-e611-8105-e0071b66a691", UnitID = "b65f38c4-464c-428a-b67e-274fa06d7a7f"}
                }
            };
            return View(dummy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client.Types.Order4Creation content)
        {
            if (ModelState.IsValid)
            {
                // content is not used at all but this dummy
                Client.Types.Order4Creation dummy = new Client.Types.Order4Creation
                {
                    CustomerID = "84cc87f3-ad62-e611-80e3-c4346bc516e8",
                    PriceLevelID = "0c407dd9-1b59-e611-80e2-c4346bc58784",
                    Name = "A test order created by ASP.NET",
                    Description = "Created by code, delete it after use",
                    OrderID = "test 1111",
                    OrderedProducts = new List<Client.Types.OrderDetail> {
                        new Client.Types.OrderDetail { Quantity = 1, ProductID = "09e6d87e-46ef-e611-8105-e0071b66a691", UnitID = "b65f38c4-464c-428a-b67e-274fa06d7a7f"}
                    }
                };
                try
                {
                    Guid newID = await entity.Create<Client.Types.Order4Creation>(dummy);
                    _logger.LogInformation($"Created new contact with id = {newID.ToString()}");

                    // Use this id and Contact ID to create role in Connection
                    // _record1id_value = order id
                    // _record2id_value = contact id
                    // record2objecttypecode = 2
                    // name = person's name
                    // _record2roleid_value = 8355863e-85fc-e611-810b-e0071b6685b1
                    Client.Types.Connection conn = new Client.Types.Connection
                    {
                        OrderID = newID.ToString(),
                        ContactID = "5f880511-b362-e611-80e3-c4346bc43f98",
                        RoleID = "8355863e-85fc-e611-810b-e0071b6685b1"
                    };
                    // This is an optional step?
                    Client.Entities.Connection connection = new Client.Entities.Connection(entity.Connector);
                    await connection.Create<Client.Types.Connection>(conn);
                    return RedirectToAction("Get", new { id = newID });
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex.ToString());
                }
                return RedirectToAction("Index");
            }
            return View(content);
        }
    }
}
