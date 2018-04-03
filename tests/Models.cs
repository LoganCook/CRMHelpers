using System;
using System.IO;
using Xunit;
using Microsoft.AspNetCore.WebUtilities;
using Client.Types;
using System.Json;

namespace tests
{
    public class Models
    {
        const string testGuid = "5a03318f-b812-e711-8117-480fcff1dae1";

        [Fact]
        public void ValidateTwoArgQueryBuild()
        {
            string builtQuery = Client.Entities.Query.Build(new string[] { "accountid" }, "name eq some");
            var query = QueryHelpers.ParseQuery(builtQuery);
            Console.WriteLine(query.Count);
            foreach (var k in query.Keys)
            {
                Console.WriteLine(k);
            }
            Assert.Equal(2, query.Count);
            Assert.True(query.ContainsKey("$select"));
            Assert.True(query.ContainsKey("$filter"));
        }

        #region Contact
        [Theory]
        [InlineData("new user name")]
        public void ContactCanUpdateUsernameOnly(string username)
        {
            ContactBase forUpdate = new ContactBase
            {
                Username = username
            };
            JsonObject recover = (JsonObject)JsonValue.Parse(Client.CRMClient.SerializeObject<ContactBase>(forUpdate));
            Assert.Single(recover);
            Assert.Equal(username, recover["new_username"]);
        }

        [Theory]
        [InlineData("firstName", "lastName", "email", "username", "department", testGuid)]
        public void SimpleContactJsonString(string firstName, string lastName, string email, string username, string department, string accountID)
        {
            string json = Contact.Create(firstName, lastName, email, username, department, accountID).ToString();
            JsonValue recover = JsonValue.Parse(json);
            Assert.Equal(6, recover.Count);
            Assert.True(recover.ContainsKey("firstname"));
            Assert.True(recover.ContainsKey("lastname"));
            Assert.True(recover.ContainsKey("emailaddress1"));
            Assert.True(recover.ContainsKey("new_username"));
            Assert.Equal($"/accounts({testGuid})", recover["parentcustomerid_account@odata.bind"]);
        }

        [Theory]
        [InlineData("firstName", "lastName", "email", "username", "department", testGuid)]
        public void ContactBaseJsonString(string firstName, string lastName, string email, string username, string department, string accountID)
        {
            ContactBase contact = new ContactBase
            {
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
                Username = username,
                Department = department
            };
            string json = contact.Create(accountID).ToString();
            JsonValue recover = JsonValue.Parse(json);
            Assert.Equal(6, recover.Count);
            Assert.True(recover.ContainsKey("firstname"));
            Assert.True(recover.ContainsKey("lastname"));
            Assert.True(recover.ContainsKey("emailaddress1"));
            Assert.True(recover.ContainsKey("new_username"));
            Assert.Equal($"/accounts({testGuid})", recover["parentcustomerid_account@odata.bind"]);
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
            JsonObject recover = (JsonObject)JsonValue.Parse(Client.CRMClient.SerializeObject<Order4Creation>(order));
            Assert.Equal(typeof(Order4Creation).GetProperties().Length, recover.Count);
            Assert.True(recover.ContainsKey("customerid_account@odata.bind"));
            Assert.Equal($"/accounts({testGuid})", recover["customerid_account@odata.bind"]);
            Assert.Equal($"/pricelevels({testGuid})", recover["pricelevelid@odata.bind"]);
            Assert.Equal("test", recover["name"]);
            Assert.Equal("description", recover["description"]);
            Assert.Equal("new_orderid", recover["new_orderid"]);
        }

        [Fact]
        public void OrderdetailSimpleCanDeserialize()
        {
            // PriceLocked default false
            // ParentbundleID default null
            OrderdetailSimple orderDetail = new OrderdetailSimple
            {
                ID = testGuid,
                Product = "testProduct",
                ProductID = testGuid,
                UnitPrice = 0.1F,
                Discount = 0F,
                PriceLocked = false,
                Quantity = 1,
                UnitID = testGuid,
                Unit = "Year",
                ProductType = "2",
                ParentbundleID = testGuid
            };

            string jsonString = Client.CRMClient.SerializeObject<OrderdetailSimple>(orderDetail);
            JsonObject recover = (JsonObject)JsonValue.Parse(jsonString);
            // ProductType is omitted
            Assert.Equal(typeof(OrderdetailSimple).GetProperties().Length, recover.Count);
            Assert.Equal("Bundle", orderDetail.ProductType);
            JsonObject obj = new JsonObject
            {
                { "salesorderdetailid", testGuid },
                { "_productid_value@OData.Community.Display.V1.FormattedValue", "DemoProduct" },
                { "_productid_value", testGuid },
                { "priceperunit", 0 },
                { "salesorderispricelocked", false },
                { "manualdiscountamount_base", 0 },
                { "quantity", 1 },
                { "_uomid_value", testGuid},
                { "_uomid_value@OData.Community.Display.V1.FormattedValue", "Year" },
                { "producttypecode", 2 },
                { "parentbundleid", null }
            };
            using (MemoryStream stringStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(obj.ToString())))
            {
                OrderdetailSimple deserialized = Client.CRMClient.DeserializeObject<OrderdetailSimple>(stringStream);
                Assert.Equal("Bundle", deserialized.ProductType);
                Assert.True(deserialized.PriceLocked == false);
                Assert.Equal<float>(0, deserialized.Discount);
                Assert.Equal<int>(1, deserialized.Quantity);
            }
        }

        #endregion

        #region Account
        [Fact]
        public void AccountCanDeserialize()
        {
            Account account = new Account
            {
                ID = testGuid,
                Name = "testAccount",
                ParentAccountID = testGuid,
                ParentAccount = "null"
            };
            string jsonString = Client.CRMClient.SerializeObject<Account>(account);
            JsonObject recover = (JsonObject)JsonValue.Parse(jsonString);
            Assert.Equal(typeof(Account).GetProperties().Length, recover.Count);
            Assert.True(recover.ContainsKey("accountid"));
            Assert.True(recover.ContainsKey("name"));
            Assert.True(recover.ContainsKey("_parentaccountid_value"));
            Assert.Equal(account.ID, recover["accountid"]);
            Assert.Equal(account.Name, recover["name"]);
            Assert.Equal(account.ParentAccountID, recover["_parentaccountid_value"]);

            using (MemoryStream stringStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString))) {
                Account deserialized = Client.CRMClient.DeserializeObject<Account>(stringStream);
                Assert.Equal(account.ID, deserialized.ID);
                Assert.Equal(account.Name, deserialized.Name);
                Assert.Equal(account.ParentAccountID, deserialized.ParentAccountID);
            }
        }
        #endregion

        #region ProductPricelist
        [Fact]
        public void ProductPricelistCanDeserialize()
        {
            // Tests run from bin directory
            using (StreamReader raw = new StreamReader("../../../productpricelists.json"))
            {
                var pplist = Client.CRMClient.DeserializeObject<OData<ProductPricelist>>(raw.BaseStream).Value;
                // current test data productpricelists.json has 42 entries
                Assert.Equal(42, pplist.Count);
            }
        }
        #endregion
    }
}
