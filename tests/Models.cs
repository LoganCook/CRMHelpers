using System;
using Xunit;
using Microsoft.AspNetCore.WebUtilities;
using Synchroniser.Models;
using System.Json;

namespace tests
{
    public class Models
    {
        static readonly string testGuid = "5a03318f-b812-e711-8117-480fcff1dae1";

        #region Product
        [Fact]
        public void UriShouldValid()
        {
            string uri = Product.Get("tester");
            Assert.StartsWith("products", uri);
            Console.WriteLine(uri);
            var query = QueryHelpers.ParseQuery(uri.Replace("products?", ""));
            Assert.Equal(query.Count, 2);
            Assert.True(query.ContainsKey("$filter"));
            Assert.True(query.ContainsKey("$select"));
        }
#endregion

        #region Contact
        [Theory]
        [InlineData("fullname", "email")]
        public void SimpleContactJsonString(string fullName, string email)
        {
            string json = SimpleContact.Create(fullName, email);
            JsonValue recover = JsonValue.Parse(json);
            Assert.Equal(2, recover.Count);
            Assert.True(recover.ContainsKey("fullname"));
            Assert.True(recover.ContainsKey("emailaddress1"));
        }
#endregion

        #region Order
        [Fact]
        public void OrderCanBeSerialized()
        {
            Order4Creation order = new Order4Creation
            {
                Name = "test",
                CustomerID = testGuid,
                PriceLevelID = testGuid,
                Description = "description",
                OrderID = "new_orderid"
            };
            JsonObject recover = (JsonObject)JsonValue.Parse(Synchroniser.CRMClient.SerializeObject<Order4Creation>(order));
            Assert.Equal(typeof(Order4Creation).GetProperties().Length, recover.Count);
            Assert.True(recover.ContainsKey("customerid_account@odata.bind"));
            Assert.Equal($"/accounts({testGuid})", recover["customerid_account@odata.bind"]);
            Assert.Equal($"/pricelevels({testGuid})", recover["pricelevelid@odata.bind"]);
            Assert.Equal("test", recover["name"]);
            Assert.Equal("description", recover["description"]);
            Assert.Equal("new_orderid", recover["new_orderid"]);
        }
        #endregion
    }
}
