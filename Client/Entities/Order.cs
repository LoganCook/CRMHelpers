using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class Order : Base
    {
        public const string ENTITY = "salesorder";

        public Order(CRMClient conn) : base(conn)
        {
            ENDPOINT = "salesorders";
            commonFileds = new string[] { "salesorderid", "new_orderid", "description", "name", "statecode", "_customerid_value" };
        }

        #region public queries work with query (utility) methods in Base
        /// <summary>
        /// Get order by its eRSA order ID
        /// </summary>
        /// <param name="id">eRSA order ID: Cloud blar blar, or GOSA0001 etc</param>
        /// <returns>Basic information of an Order and Orderline information</returns>
        /* With filter, expand returns nextLink: Retrieve related entities by expanding collection-valued navigation properties
         * {
              "@odata.context":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/$metadata#salesorders(salesorderid,new_orderid,description,name,statecode,_customerid_value,order_details,order_details(_productid_value,priceperunit,quantity,manualdiscountamount_base,producttypecode,_uomid_value))","value":[
                {
                  "@odata.etag":"W/\"9960853\"","salesorderid":"e2c5a386-a821-e811-8131-480fcff12ac1","new_orderid":"lidemoHPCwithPrioritisedQueue","description":null,"name":"Demo of HPC with Prioritised Queue","statecode":0,"_customerid_value":"c129372b-5063-e611-80e3-c4346bc4de3c",
                  "order_details":[],
                  "order_details@odata.nextLink":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/salesorders(e2c5a386-a821-e811-8131-480fcff12ac1)/order_details?$select=_productid_value,priceperunit,quantity,manualdiscountamount_base,producttypecode,_uomid_value"
                }
              ]
            }
        */
        public string GetByOrderIDQuery(string id)
        {
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$filter", $"new_orderid eq '{id}'"},
                { "$expand", $"order_details($select=_productid_value,priceperunit,quantity,manualdiscountamount_base,producttypecode,_uomid_value)" }
            };
            return Query.Build(parts);
        }

        // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/salesorders(e2c5a386-a821-e811-8131-480fcff12ac1)/order_details?$select=_productid_value,priceperunit,quantity,manualdiscountamount_base,producttypecode,_uomid_value
        /// <summary>
        /// Get order by its ID with products information: name and type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetByIDQuery(string id)
        {
            return $"({id})/order_details?$select=_productid_value,priceperunit,quantity,manualdiscountamount_base,producttypecode,_uomid_value";
        }

        #endregion

        #region private queries
        private string GetByNameQuery(string name)
        {
            return Query.Build(commonFileds, $"?$filter=name eq '{name}'");
        }

        /// <summary>
        /// Query to get orders of a contact by its ID
        /// </summary>
        /// <param name="contactID"></param>
        /// <returns></returns>
        private string GetOrdersOfQuery(Guid contactID)
        {
            /*
            <fetch version="1.0" mapping="logical">
                <entity name="salesorder">
                    <attribute name="name" />
                    <attribute name="description" />
                    <attribute name="new_orderid" />
                    <link-entity name="connection" from="record1id" to="salesorderid">
                        <filter type="and">
                            <condition attribute="record1objecttypecode" operator="eq" value="1088" />
                            <condition attribute="record2objecttypecode" operator="eq" value="2" />
                        </filter>
                        <link-entity name="connectionrole" from="connectionroleid" to="record2roleid">
                            <attribute name="name" alias="role" />
                        </link-entity>
                        <link-entity name="contact" from="contactid" to="record2id">
                            <filter type="and">
                                <condition attribute="contactid" operator="eq" value="12ed419f-5863-e611-80e3-c4346bc516e8" />
                            </filter>
                        </link-entity>
                    </link-entity>
                </entity>
            </fetch>
            */

            var xml = new FetchXML(ENTITY);
            FetchElement entity = xml.EntityElement;
            entity.AddField("name")
                .AddField("description")
                .AddField("new_orderid");

            FetchElement linkedConnection = entity.AddLinkEntity("connection", "record1id", "salesorderid");
            FetchElement connectionFilter = linkedConnection.AddFilter();
            connectionFilter.AddCondition("record1objecttypecode", "eq", "1088")
                .AddCondition("record2objecttypecode", "eq", "2");
            linkedConnection.AddLinkEntity("connectionrole", "connectionroleid", "record2roleid")
                .AddField("name", "role");

            FetchElement linkedContact = linkedConnection.AddLinkEntity("contact", "contactid", "record2id");
            FetchElement contactFilter = linkedContact.AddFilter();
            contactFilter.AddCondition("contactid", "eq", contactID.ToString());

            return xml.ToQueryString();
        }

        /// <summary>
        /// A link-entity which can be added to the main order query to get names of ordered product
        /// </summary>
        /// <returns></returns>
        // Not tested used yet
        private FetchElement OrderLineQuery()
        {
            var attributes = new Dictionary<string, string>
            {
                { "name", "salesorderdetail" },
                { "from", "salesorderid" },
                { "to", "salesorderid" }
            };
            FetchElement linkedOrderLine = new FetchElement("link-entity", attributes);
            linkedOrderLine.AddField("priceperunit")
                .AddField("quantity")
                .AddField("manualdiscountamount_base");

            FetchElement linkedProduct = linkedOrderLine.AddLinkEntity("product", "productid", "productid");
            linkedProduct.AddField("name", "product");
            return linkedOrderLine;
        }
        #endregion

        public Task<List<Types.Order>> ListOrdersOfContact(Guid contactID)
        {
            return List<Types.Order>(GetOrdersOfQuery(contactID));
        }
    }
}
