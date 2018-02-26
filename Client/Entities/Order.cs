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

        #region private queries
        private string GetByNameQuery(string name)
        {
            // because this is a search, it returns a list with 0 or more records
            // this is different to using id
            return $"salesorders?$filter=name eq '{name}'";
        }

        /// <summary>
        /// Get order by its eRSA order ID
        /// </summary>
        /// <param name="id">eRSA order ID: Cloud blar blar, or GOSA0001 etc</param>
        /// <returns></returns>
        private string GetByOrderIDQuery(string id)
        {
            return $"salesorders?$filter=new_orderid eq '{id}'";
        }

        /// <summary>
        /// Query to get orders of a contact by its ID
        /// </summary>
        /// <param name="contactID"></param>
        /// <returns></returns>
        private string GetOrdersOfQuery(Guid contactID)
        {
            // TODO: Connection role will not change much but to be perfect and complete we
            // need connectionrole : https://msdn.microsoft.com/en-us/library/mt607607.aspx
            // PROJECT_ADMIN_ROLE = '8355863e-85fc-e611-810b-e0071b6685b1'
            /*
            <fetch mapping='logical'>
                <entity name='salesorder'>
                    <attribute name='name' />
                    <link-entity name="connection" from="record1id" to="salesorderid">
                        <filter type='and'>
                            <condition attribute='record1objecttypecode' operator='eq' value='1088' />
                            <condition attribute='record2objecttypecode' operator='eq' value='2' />
                            <condition attribute='record2roleid' operator='eq' value='8355863e-85fc-e611-810b-e0071b6685b1' />
                        </filter>
                        <link-entity name='contact' from='contactid' to='record2id'>
                            <filter type="and">
                                <condition attribute='contactid' operator='eq' value='5f880511-b362-e611-80e3-c4346bc43f98' />
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
                .AddCondition("record2objecttypecode", "eq", "2")
            // FIXME: hard-coded role id
                .AddCondition("record2roleid", "eq", "8355863e-85fc-e611-810b-e0071b6685b1");

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
