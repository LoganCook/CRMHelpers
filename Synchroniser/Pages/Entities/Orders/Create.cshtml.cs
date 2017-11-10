using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using Synchroniser.Models;


namespace Synchroniser.Pages.Entities.Orders
{
    public class CreateModel : PageModel
    {
        private readonly ITokenConsumer _crmClient;
        //public Order4Creation dummy;
        public Order4Creation dummy = new Order4Creation
        {
            CustomerID = "84cc87f3-ad62-e611-80e3-c4346bc516e8",
            PriceLevelID = "0c407dd9-1b59-e611-80e2-c4346bc58784",
            Name = "A test order created by ASP.NET",
            Description = "Created by code, delete it after use",
            OrderID = "test 1111",
            OrderedProducts = new List<OrderDetail> {
                new OrderDetail { Quantity = 1, ProductID = "09e6d87e-46ef-e611-8105-e0071b66a691", UnitID = "b65f38c4-464c-428a-b67e-274fa06d7a7f"}
            }
        };

        public CreateModel(ITokenConsumer crmClient)
        {
            _crmClient = crmClient;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost() {
            Console.WriteLine("Create method has been called");
            HttpResponseMessage result = await _crmClient.SendJsonAsync<Order4Creation>(HttpMethod.Post, Order4Creation.URI, dummy);
            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                string newOrderID = result.Headers.GetValues("OData-EntityId").FirstOrDefault();
                Console.WriteLine("New entity: {0}", newOrderID);
                // Use this id and Contact ID to create role in Connection
                // _record1id_value = order id
                // _record2id_value = contact id
                // record2objecttypecode = 2
                // name = person's name
                // _record2roleid_value = 8355863e-85fc-e611-810b-e0071b6685b1
                Connection conn = new Connection
                {
                    OrderID = Utils.GetIdFromUrl(newOrderID),
                    ContactID = "5f880511-b362-e611-80e3-c4346bc43f98",
                    RoleID = "8355863e-85fc-e611-810b-e0071b6685b1"
                };
                // This is an optional step?
                HttpResponseMessage cresult = await _crmClient.SendJsonAsync<Connection>(HttpMethod.Post, Connection.URI, conn);
                if (cresult.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    Utils.DisplayResponse(cresult);
                }
                return Redirect($"/Entities/Orders/?orderid={dummy.OrderID}");
            }

            Utils.DisplayResponse(result);
            return Redirect("/");
        }
    }
}