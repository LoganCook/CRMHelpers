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
        }

        #region public queries work with query (utility) methods in Base
        /// <summary>
        /// Get order by its eRSA order ID
        /// </summary>
        /// <param name="id">eRSA order ID: Cloud blar blar, or GOSA0001 etc</param>
        /// <returns></returns>
        public string GetByOrderIDQuery(string id)
        {
            return $"?$filter=new_orderid eq '{id}'";
        }
        #endregion

        #region private queries
        private string GetByNameQuery(string name)
        {
            return $"?$filter=name eq '{name}'";
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
        #endregion

        public async Task<List<Types.Order>> ListOrdersOfContact(Guid contactID)
        {
            return await List<Types.Order>(GetOrdersOfQuery(contactID));
        }

    }
}
