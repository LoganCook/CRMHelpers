using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Json;
using Microsoft.AspNetCore.WebUtilities;
using Synchroniser.Models;

/* Console.Write* only works when:
 * There are errors,
 * -v is one of n, d, diag
 */
namespace tests
{
    [TestClass]
    public class MSTests
    {
        static readonly string testGuid = "5a03318f-b812-e711-8117-480fcff1dae1";

        [TestMethod]
        public void VaidateProductGet()
        {
            string uri = Product.Get("tester");
            StringAssert.StartsWith(uri, "products");
            Console.WriteLine(uri);
            var query = QueryHelpers.ParseQuery(uri.Replace("products?", ""));
            Console.WriteLine(query.Count);
            foreach( var k in query.Keys)
            {
                Console.WriteLine(k);
            }
            Assert.IsTrue(query.ContainsKey("$filter"));
        }

        [TestMethod]
        public void OrderCanBeSerialized()
        {
            List<OrderDetail> orderedProducts = new List<OrderDetail>
            {
                new OrderDetail { Quantity = 1, ProductID = testGuid, UnitID = testGuid },
                new OrderDetail { Quantity = 1, ProductID = testGuid, UnitID = testGuid }
            };
            Order4Creation order = new Order4Creation
            {
                Name = "test",
                CustomerID = testGuid,
                PriceLevelID = testGuid,
                Description = "description",
                OrderID = "new_orderid",
                OrderedProducts = orderedProducts
            };
            JsonObject recover = (JsonObject)JsonValue.Parse(Synchroniser.CRMClient.SerializeObject<Order4Creation>(order));
            Console.WriteLine(recover.ToString());
            Assert.AreEqual(typeof(Order4Creation).GetProperties().Length, recover.Count);
            Assert.IsTrue(recover.ContainsKey("customerid_account@odata.bind"));
            Assert.AreEqual<string>($"/accounts({testGuid})", recover["customerid_account@odata.bind"]);
            Assert.AreEqual<string>($"/pricelevels({testGuid})", recover["pricelevelid@odata.bind"]);
            Assert.AreEqual<string>("test", recover["name"]);
            Assert.AreEqual<string>("description", recover["description"]);
            Assert.AreEqual<string>("new_orderid", recover["new_orderid"]);
            Assert.IsTrue(recover.ContainsKey("order_details"));
            Assert.AreEqual(orderedProducts.Count, recover["order_details"].Count);
            for (int i = 0; i < orderedProducts.Count; i++)
            {
                Assert.AreEqual<int>(1, recover["order_details"][i]["quantity"]);
                Assert.AreEqual<string>($"/products({testGuid})", recover["order_details"][i]["productid@odata.bind"]);
                Assert.AreEqual<string>($"/uoms({testGuid})", recover["order_details"][i]["uomid@odata.bind"]);
            }
        }

        [TestMethod]
        public void ConnectionCanBeSerialized()
        {
            Connection connn = new Connection
            {
                OrderID = testGuid,
                ContactID = testGuid,
                RoleID = testGuid
            };
            JsonObject recover = (JsonObject)JsonValue.Parse(Synchroniser.CRMClient.SerializeObject<Connection>(connn));
            Assert.AreEqual(typeof(Connection).GetProperties().Length, recover.Count);
        }
    }
}
